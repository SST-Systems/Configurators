# Configurators — продвинутое

[English](ADVANCED.md) | **Русский** · [← назад к README](../README.ru.md)

Этот документ — углублённая часть, вынесенная из README: тонкий контроль лайфтайма и внутреннее устройство менеджеров. Для старта он не нужен — читай, когда нужно предсказывать пограничные случаи или брать управление лайфтаймом на себя.

- [Хуки жизненного цикла биндинга](#хуки-жизненного-цикла-биндинга)
- [Под капотом](#под-капотом)

---

## Хуки жизненного цикла биндинга

Любой конфигуратор — элемент списка процессора, будь то modification, instruction, condition или extension — может взять управление собственным лайфтаймом биндинга, реализовав `IBindingLifecycle`. Тогда менеджер вызывает `OnResolve()` при резолве биндинга и `OnRelease()` при его диспозе (при повторном резолве того же процессора или при уничтожении `lifetimeOwner`). Это полностью opt-in: конфигураторы, не реализующие интерфейс, ни в каких колбэках не участвуют.

Используй это для resolve-scoped состояния, которое нужно поднимать и разбирать вместе с биндингом — чаще всего для подписок на события:

```csharp
[Serializable]
public class PlaySound : Instruction, IBindingLifecycle
{
    [Inject] private IAudioBus _audioBus;

    void IBindingLifecycle.OnResolve() => _audioBus.MuteChanged += OnMuteChanged;
    void IBindingLifecycle.OnRelease() => _audioBus.MuteChanged -= OnMuteChanged;

    public override void Apply() { /* проиграть звук */ }

    private void OnMuteChanged(bool muted) { /* среагировать */ }
}
```

Гарантии:

- **Парность 1:1** — за каждым `OnResolve()` следует ровно один `OnRelease()`. Повторный резолв сначала диспозит старый биндинг (`OnRelease`), затем строит новый (`OnResolve`).
- **Порядок** — `OnResolve()` вызывается *после* привязки пуленого хендлера (если он есть), поэтому handler-based конфигуратор может рассчитывать, что хендлер уже прикреплён.
- **Композиты** — условия, вложенные в `All`/`Any`/`None`/`Not`, тоже получают хуки; каждый уникальный конфигуратор вызывается один раз, даже если он общий или доступен через композиты.
- **Держи состояние подписки на конфигураторе, а не на пуленом хендлере** — хендлеры общие и переиспользуются между резолвами, поэтому подписка, сохранённая там, протечёт.

`OnResolve()` — твой код и может бросить исключение; оно пробрасывается из вызова `Resolve*`, как и сбой привязки хендлера (не проглатывается). Поскольку `OnRelease` регистрируется только после возврата из `OnResolve`, бросивший `OnResolve` никогда не оставит висячий `OnRelease`.

---

## Под капотом

Этот раздел описывает что именно происходит внутри менеджера при каждом вызове. Понимание этого помогает предсказывать пограничные случаи и уверенно использовать API.

### Пул хендлеров

Каждый менеджер владеет `MultiPool<Type, IXxxHandler>` — пулом с ключом по типу хендлера. При первом запросе типа `factory.Create(type)` вызывается один раз и регистрируется фабричный делегат; все последующие запросы возвращают пулящийся экземпляр. При диспозе хендлеры анбиндятся из data-объектов и возвращаются в пул. Хендлеры **не** сбрасываются при возврате — если хендлер накапливает состояние, сбрасывай его в `SetData` или `Apply`.

### Реестр активных биндингов

Каждый менеджер хранит `Dictionary<object, IDisposable> ActiveBindings` — соответствие процессора его текущему биндингу. При повторном вызове любого `Resolve*` на том же процессоре словарь находит предыдущий биндинг и тихо диспозит его перед созданием нового.

### ProcessorDisposable — LIFO-очистка

`IDisposable`, возвращаемый из `Resolve*` — это `ProcessorDisposable`: упорядоченный список action-ов очистки. При `Dispose()` они выполняются **в обратном (LIFO) порядке**. Каждый action обёрнут в try/catch, поэтому один сбой не блокирует остальные.

Для `ResolveInstructions` порядок регистрации:

1. `cancellationTokenSource.Dispose()`
2. `binding.Dispose()` — анбиндит хендлеры, удаляет из ActiveBindings
3. `cancellationTokenSource.Cancel()`

Reversed при диспозе → **Cancel → Unbind хендлеров → Dispose CTS**. Отмена должна распространиться в выполняющуюся таску прежде чем хендлеры освободятся.

`ProcessorDisposable` идемпотентен: первый `Dispose()` выставляет флаг, последующие вызовы — no-op. Если `Register()` вызывается на уже задиспоженном экземпляре (гонка с уничтожением `lifetimeOwner`), action срабатывает немедленно — ничего не утекает.

### ProcessorReleaser — привязка лайфтайма

`ProcessorReleaser` — внутренний `MonoBehaviour`, добавляемый к `lifetimeOwner.gameObject` при первом использовании (`TryGetComponent`, иначе `AddComponent`). Он хранит список `IDisposable`-биндингов и вызывает `Dispose()` на каждом — в обратном порядке — внутри `OnDestroy`. Атрибут `[DisallowMultipleComponent]` гарантирует наличие только одного релизера на GameObject; несколько менеджеров на одном owner-е разделяют один компонент.

Если GameObject уже уничтожен в момент вызова `Add()` (гонка), релизер диспозит входящий биндинг немедленно, а не молча его дропает.

### Полный жизненный цикл ResolveInstructions

```
ResolveInstructions(processor, lifetimeOwner)   // биндит хендлеры; НЕ запускает их
│
├─ 1. processor == null? → лог ошибки, return null
│
├─ 2. Есть предыдущий биндинг для этого процессора? → Dispose() (Cancel + Unbind + Dispose CTS)
│
├─ 3. new ProcessorDisposable; ActiveBindings[processor] = disposable
│
├─ 4. new CancellationTokenSource cts; processor.SetResolveCancellation(cts.Token)
│
├─ 5. foreach инструкция в processor.Instructions
│       инструкция — IHandlerBinder?
│         pool.HasFactory(type)? нет → RegisterFactory(() => factory.Create(type))
│         pool.Get(type) → пулящийся или свежесозданный экземпляр
│         binder.BindHandler(handler) → data сохраняет ссылку на хендлер
│
├─ 6. Регистрация очистки (выполнится LIFO): cts.Dispose → Unregister(processor) → cts.Cancel
│
└─ 7. BindLifetime(disposable, lifetimeOwner)
        TryGetComponent<ProcessorReleaser>, иначе AddComponent
        releaser.Add(disposable)

// Отдельный шаг, вызывается пользователем — НЕ часть резолва:
processor.Apply(cancellationToken)
│
├─ отменяет/диспозит свой предыдущий прогон, затем линкует токен вызывающего с resolve-токеном
├─ ExecutionTask = RunAsync(linkedToken)
└─ RunAsync: foreach инструкция
       cancellationToken.ThrowIfCancellationRequested()
       инструкция — IAsyncInstruction ? await Apply(token) : Apply()
       OperationCanceledException → re-throw (останавливает цепочку)
       другое исключение          → Debug.LogException (продолжает)
```

### Цепочка диспоза

```
combined.Dispose()
  — вызывается: пользовательским кодом, lifetimeOwner.OnDestroy, или повторным ResolveInstructions
│
├─ 1. cancellationTokenSource.Cancel()
│       токен становится отменённым
│       Apply() текущей инструкции бросает OperationCanceledException
│       RunAsync пробрасывает исключение → ExecutionTask завершается с OperationCanceledException
│
├─ 2. binding.Dispose() → Unregister(processor)
│       ActiveBindings.Remove(processor)
│       foreach инструкция — IHandlerBinder
│           binder.UnbindHandler() → data обнуляет ссылку на хендлер
│           pool.Release(type, handler) → хендлер возвращается в пул
│
└─ 3. cancellationTokenSource.Dispose()
        освобождает OS wait handle, связанный с токеном
```
