<img src="Documentation~/banner.png" width="900" alt="Configurators">

[![release](https://img.shields.io/github/v/release/SST-Systems/Configurators)](../../releases)
[![release date](https://img.shields.io/github/release-date/SST-Systems/Configurators)](../../releases)
[![last commit](https://img.shields.io/github/last-commit/SST-Systems/Configurators)](../../commits)
[![license](https://img.shields.io/github/license/SST-Systems/Configurators)](LICENSE.md)

[English](README.md) | **Русский**

---

Настраивай игровые правила прямо в инспекторе — без изменений кода при добавлении новых поведений. Условия, эффекты, модификации и расширения — всё сериализуемо, пулится и работает с любым DI-контейнером.

## Содержание

<details>
<summary>Развернуть</summary>

- [Установка](#установка)
- [Концепции](#концепции)
- [Инициализация](#инициализация)
  - [Создание менеджеров](#создание-менеджеров)
  - [Создание фабрики хендлеров](#создание-фабрики-хендлеров)
- [Модули](#модули)
  - [Modifications](#модуль-modifications)
  - [Instructions](#модуль-instructions)
  - [Conditions](#модуль-conditions)
  - [Extensions](#модуль-extensions)
- [Инспектор](#инспектор)
- [Контракт лайфтайма](#контракт-лайфтайма)
  - [Хуки жизненного цикла биндинга](#хуки-жизненного-цикла-биндинга)
- [Под капотом](#под-капотом)
- [Утилиты в комплекте](#утилиты-в-комплекте)

</details>

---

## Установка

1. **.unitypackage** — [Releases](../../releases)
2. **UPM** — `Window → Package Manager` → `+` → `Add package from git URL`. UPM не резолвит git-зависимости автоматически — добавь все три:
   - [Pooling](https://github.com/SST-Systems/Pooling): `https://github.com/SST-Systems/Pooling.git`
   - [StableRef](https://github.com/SST-Systems/StableRef): `https://github.com/SST-Systems/StableRef.git`
   - Configurators: `https://github.com/SST-Systems/Configurators.git`

   Добавь `#тег` в конец каждого URL для фиксации версии.
3. **Вручную** — склонируй или скачай все три репозитория, скопируй в `Assets/`.

Unity 2021.3+

> Опциональная интеграция с Zenject включена как sample — импортируй через `Window → Package Manager` → выбери Configurators → вкладка **Samples**.

---

## Концепции

| Термин | Что это |
|---|---|
| **Modification** | Единица работы, применяемая к `TContext`, выполняется последовательно. Например: «выставить max HP», «добавить тег». |
| **Instruction** | Команда без контекста — цели хранятся прямо в инспекторных полях, выполняется последовательно. Например: `GameObjectSetActive`, `PlaySound`, `WaitForSeconds`. |
| **Condition** | Булево условие с подпиской на изменения (`AddListener`) и прямой проверкой (`IsMet`). |
| **Extension** | Носитель значения, прикреплённый к конфигу или компоненту, читается по запросу. Например: кулдаун, максимальное количество, иконка. |
| **Processor** | Контейнер со списком элементов одного модуля. Лежит на конфиге или компоненте. |
| **Handler** | Пулящаяся runtime-логика для data-объекта. Нужен когда требуются внешние зависимости или инжект через DI. |
| **HandlerFactory** | Контролирует создание хендлеров. По умолчанию — `Activator.CreateInstance`. |

---

## Инициализация

### Создание менеджеров

Каждый модуль имеет свой менеджер. Создай нужные и держи их на протяжении жизни сцены или проекта:

```csharp
IInstructionManager instructionManager = new InstructionManager();
IModificationManager modificationManager = new ModificationManager();
IConditionManager conditionManager = new ConditionManager();
IExtensionManager extensionManager = new ExtensionManager();
```

Каждый компонент инжектирует только тот интерфейс, который ему нужен.

### Создание фабрики хендлеров

Хендлеры — это пулируемые runtime-объекты: создаются один раз, переиспользуются и возвращаются в пул при диспозе. Из-за этого их нельзя создавать через стандартный DI-контейнер напрямую: контейнер не знает, когда их создавать и сколько экземпляров нужно. Фабрика решает эту проблему — она единственное место, которое знает как сконструировать хендлер, и может делегировать это DI-контейнеру, чтобы тот автоматически проинжектировал все зависимости.

Менеджеры делегируют создание хендлеров в `IHandlerFactory`:

```csharp
public interface IHandlerFactory
{
    object Create(Type handlerType);
}
```

**По умолчанию — `ActivatorHandlerFactory`** — создаёт хендлеры через `Activator.CreateInstance`. Работает когда у хендлеров нет зависимостей (или они получают их вручную через сервис-локатор).

**Кастомная фабрика** — реализуй `IHandlerFactory`, чтобы DI-контейнер сам создавал хендлеры и инжектировал зависимости:

```csharp
public class ZenjectHandlerFactory : IHandlerFactory
{
    [Inject] private readonly IInstantiator _instantiator;

    public object Create(Type handlerType) => _instantiator.Instantiate(handlerType);
}
```

После этого любой хендлер может объявить собственные поля с `[Inject]` и получать зависимости как любой другой класс — фабрика возьмёт это на себя.

#### Zenject

```csharp
public class ConfiguratorsZenjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IHandlerFactory>().To<ZenjectHandlerFactory>().AsSingle();

        Container.Bind<IInstructionManager>().To<InstructionManager>().AsSingle();
        Container.Bind<IModificationManager>().To<ModificationManager>().AsSingle();
        Container.Bind<IConditionManager>().To<ConditionManager>().AsSingle();
        Container.Bind<IExtensionManager>().To<ExtensionManager>().AsSingle();
    }
}
```

Готовая интеграция — в [`Samples~/Zenject/`](Samples~/Zenject/).

---

## Модули

Основная единица хранения конфигураторов — **Processor**. В нём хранится и настраивается список элементов прямо в инспекторе. Для каждого модуля есть свой тип процессора (`ModificationProcessor<T>`, `InstructionProcessor`, `ConditionProcessor`, `ExtensionProcessor`) — добавь нужный на конфиг или компонент.

После объявления процессор необходимо зарезолвить через менеджер — это привязывает хендлеры к data-объектам и подготавливает runtime. Лайфтайм контролируется через `lifetimeOwner` (авто-диспоз при уничтожении объекта) или вручную через возвращённый `IDisposable`.

```csharp
// 1. Создай менеджер один раз (лайфтайм сцены/проекта)
IModificationManager manager = new ModificationManager();

// 2. Резолв перед использованием — привязывает хендлеры
manager.ResolveModifications(processor, lifetimeOwner: this);

// 3. Запуск после резолва — Apply возвращает Task; await, если нужно дождаться async-шагов
await processor.Apply(context);
```

> **О примерах.** Примеры кода ниже не используют DI-контейнер. `ServiceLocator.Get<T>()` — условное обозначение: как именно ты получаешь менеджеры (ручное создание, Zenject, VContainer или любой другой подход) — полностью на твоё усмотрение. `ActorUnit`, `PlayerHealth`, `IGameFactory`, `ISkinService` и аналогичные типы — проектные плейсхолдеры; подставь свои.

### Модуль Modifications

#### 1. Зачем

Модификации применяют эффекты к контексту — объекту, сущности, любым данным. Например: настроить характеристики юнита при спавне, применить эффект предмета к игроку, изменить параметры уровня. Вся логика описывается в инспекторе без изменений кода. Модификации выполняются **последовательно**; шаг может быть синхронным или, когда нужно дождаться чего-то (загрузка ассета, получение значения), асинхронным — следующий стартует только после завершения текущего.

#### 2. Как использовать

Выбор по двум осям: **inline vs с хендлером** (нужны инжектируемые зависимости?) и **sync vs async** (нужно `await`?). Sync — по умолчанию и легче всего в написании; async бери только когда шаг реально ждёт.

- **Inline** — логика прямо на data-классе. Sync-база `Modification<TContext>` (`void Apply`), async-база `AsyncModification<TContext>` (`Task Apply`).
- **С хендлером** — данные и логика разделены; хендлер создаётся фабрикой и пулится. Sync-пара `ModificationData` + `ModificationHandler`, async-пара `AsyncModificationData` + `AsyncModificationHandler`.

Процессор выполняет список по порядку — синхронные записи сразу, асинхронные через `await`. Если все записи синхронные, весь запуск завершается синхронно.

> **Рекомендация.** По умолчанию бери sync (`Modification` / `ModificationData`); переключайся на async-варианты только когда шаг действительно ждёт.

> **Отмена.** Диспоз биндинга или уничтожение `lifetimeOwner` отменяет выполняющиеся async-запуски. Можно также передать свой токен в `Apply`. Прокидывай токен во все `await` внутри async-модификаций (и вызывай `cancellationToken.ThrowIfCancellationRequested()` в циклах), иначе текущий шаг доработает до конца прежде чем остановится.

> **Конкурентность.** Процессор не хранит состояние запуска, поэтому один процессор можно применять сразу к нескольким объектам — например `await Task.WhenAll(units.Select(u => modifications.Apply(u)))`. Цепочка каждого контекста выполняется независимо (и по порядку внутри себя). Безопасно, пока хендлеры не хранят per-call состояние в полях — контекст приходит параметром, а не сохраняется.

##### Inline — sync

```csharp
[Serializable]
[StableRefCategory("Stats")]
public class SetMaxHealth : Modification<ActorUnit>
{
    public int Value;

    public override void Apply(ActorUnit context) => context.GetAbility<HealthAbility>().SetMax(Value);
}
```

##### Inline — async

```csharp
[Serializable]
[StableRefCategory("Time")]
public class RevealAfterDelay : AsyncModification<ActorUnit>
{
    public float Seconds;

    public override async Task Apply(ActorUnit context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(Seconds), cancellationToken);
        context.SetVisible(true);
    }
}
```

##### С хендлером — sync (DI, без await)

```csharp
[Serializable]
[StableRefCategory("Spawn")]
public class SpawnChild : ModificationData<ActorUnit, SpawnChildHandler>
{
    public ActorUnit Prefab;
}

public class SpawnChildHandler : ModificationHandler<SpawnChild, ActorUnit>
{
    private readonly IGameFactory _factory = ServiceLocator.Get<IGameFactory>();
    // С Zenject: [Inject] private readonly IGameFactory _factory;

    public override void Apply(ActorUnit context) => _factory.Spawn(Data.Prefab, context.transform.position);
}
```

##### С хендлером — async (DI + await)

```csharp
[Serializable]
[StableRefCategory("Appearance")]
public class ApplySkin : AsyncModificationData<ActorUnit, ApplySkinHandler>
{
    public string SkinId;
}

public class ApplySkinHandler : AsyncModificationHandler<ApplySkin, ActorUnit>
{
    private readonly ISkinService _skins = ServiceLocator.Get<ISkinService>();
    // С Zenject: [Inject] private readonly ISkinService _skins;

    public override async Task Apply(ActorUnit context, CancellationToken cancellationToken = default)
    {
        var skin = await _skins.LoadAsync(Data.SkinId, cancellationToken);
        context.SetSkin(skin);
    }
}
```

#### 3. Пример использования

```csharp
public class ActorSpawner : MonoBehaviour
{
    [SerializeField] private ModificationProcessor<ActorUnit> modifications;

    private readonly IModificationManager _modificationManager = ServiceLocator.Get<IModificationManager>();
    // С Zenject: [Inject] private readonly IModificationManager _modificationManager;

    private void Awake()
    {
        // Резолв один раз — хендлеры привязаны, цепочка отменится при уничтожении объекта
        _modificationManager.ResolveModifications(modifications, lifetimeOwner: this);
    }

    // Сконфигурировать только что заспавненного актёра — ждём каждую модификацию по порядку
    public async Task Configure(ActorUnit actor)
    {
        try { await modifications.Apply(actor); }
        catch (OperationCanceledException) { /* актёр или спавнер уничтожен во время конфигурации */ }
    }
}
```

Ручной контроль (`lifetimeOwner: null`):

```csharp
private IDisposable _binding;

private void Setup(ModificationProcessor<SomeContext> processor)
{
    _binding = _modificationManager.ResolveModifications(processor, lifetimeOwner: null);
}

private void Cleanup()
{
    _binding?.Dispose(); // хендлеры возвращаются в пул
}
```

#### 4. Дополнительные методы

**Повторный резолв** — вызов `ResolveModifications` на уже резолвнутом процессоре автоматически диспозит предыдущий биндинг и создаёт новый. Используй чтобы передать контроль новому `lifetimeOwner`:

```csharp
_modificationManager.ResolveModifications(processor, lifetimeOwner: newOwner);
```

---

### Модуль Instructions

#### 1. Зачем

Инструкции — команды без контекста. Цели хранятся прямо в инспекторных полях data-класса. Инструкции выполняются последовательно: каждая следующая стартует только после завершения предыдущей. Шаг может быть синхронным или асинхронным (задержки, ожидания, отмена). Типичные примеры: последовательность событий на уровне, анимационные цепочки, туториальные шаги, задержки между действиями.

#### 2. Как использовать

Выбор по двум осям: **inline vs с хендлером** (нужны инжектируемые зависимости?) и **sync vs async** (нужно `await`?). Sync — по умолчанию.

- **Inline** — логика на data-классе. Sync-база `Instruction` (`void Apply()`), async-база `AsyncInstruction` (`Task Apply(ct)`).
- **С хендлером** — данные и логика разделены; хендлер создаётся фабрикой и пулится. Sync-пара `InstructionData` + `InstructionHandler`, async-пара `AsyncInstructionData` + `AsyncInstructionHandler`.

Процессор выполняет список по порядку — синхронные записи сразу, асинхронные через `await`.

> **Рекомендация.** По умолчанию бери sync (`Instruction` / `InstructionData`); async-варианты — когда шаг реально ждёт.

> **Отмена.** При диспозе биндинга или уничтожении `lifetimeOwner` текущая цепочка отменяется; повторный `Apply` отменяет предыдущий запуск. В своих async-инструкциях прокидывай токен во все `await` (`Task.Delay(ms, cancellationToken)`, `UniTask.Delay(...)` и т.д.) и вызывай `cancellationToken.ThrowIfCancellationRequested()` в циклах — иначе текущий шаг доработает до конца прежде чем остановится.

##### Inline — sync

```csharp
[Serializable]
[StableRefCategory("GameObject")]
public class GameObjectSetActive : Instruction
{
    public GameObject Object;
    public bool Value;

    public override void Apply() => Object.SetActive(Value);
}
```

##### Inline — async

```csharp
// Задержка — токен передаётся в Task.Delay, отмена работает сразу
[Serializable]
[StableRefCategory("Time")]
public class WaitForSeconds : AsyncInstruction
{
    public float Duration;

    public override async Task Apply(CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromSeconds(Duration), cancellationToken);
    }
}

// Цикл с проверкой отмены на каждой итерации
[Serializable]
[StableRefCategory("Movement")]
public class MoveToTarget : AsyncInstruction
{
    public Transform Object;
    public Transform Target;
    public float Speed = 5f;

    public override async Task Apply(CancellationToken cancellationToken = default)
    {
        while (Vector3.Distance(Object.position, Target.position) > 0.01f)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Object.position = Vector3.MoveTowards(Object.position, Target.position, Speed * Time.deltaTime);
            await Task.Yield();
        }
    }
}
```

##### С хендлером — sync (DI, без await)

```csharp
[Serializable]
[StableRefCategory("Audio")]
public class SetMasterVolume : InstructionData<SetMasterVolumeHandler>
{
    [Range(0, 1)] public float Volume = 1f;
}

public class SetMasterVolumeHandler : InstructionHandler<SetMasterVolume>
{
    private readonly IAudioService _audioService = ServiceLocator.Get<IAudioService>();
    // С Zenject: [Inject] private readonly IAudioService _audioService;

    public override void Apply() => _audioService.SetMasterVolume(Data.Volume);
}
```

##### С хендлером — async (DI + await)

```csharp
[Serializable]
[StableRefCategory("Audio")]
public class PlaySound : AsyncInstructionData<PlaySoundHandler>
{
    public AudioClip Clip;
    [Range(0, 1)] public float Volume = 1f;
}

public class PlaySoundHandler : AsyncInstructionHandler<PlaySound>
{
    private readonly IAudioService _audioService = ServiceLocator.Get<IAudioService>();
    // С Zenject: [Inject] private readonly IAudioService _audioService;

    public override async Task Apply(CancellationToken cancellationToken = default)
    {
        await _audioService.PlayAndWait(Data.Clip, Data.Volume, cancellationToken);
    }
}
```

#### 3. Пример использования

```csharp
public class TutorialController : MonoBehaviour
{
    [SerializeField] private InstructionProcessor steps;

    private readonly IInstructionManager _instructionManager = ServiceLocator.Get<IInstructionManager>();
    // С Zenject: [Inject] private readonly IInstructionManager _instructionManager;

    private void Awake()
    {
        // Резолв один раз — хендлеры привязаны, цепочка отменится при уничтожении объекта
        _instructionManager.ResolveInstructions(steps, lifetimeOwner: this);
    }

    // Запустить цепочку — повторный вызов автоматически отменяет предыдущий запуск
    public void StartTutorial()
    {
        steps.Apply();
    }

    // Запустить и дождаться завершения всей цепочки
    public async Task StartAndAwait()
    {
        steps.Apply();

        try
        {
            await steps.ExecutionTask;
            Debug.Log("Все инструкции выполнены");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Цепочка прервана");
        }
    }

    // Досрочная отмена без перезапуска
    public void StopTutorial() => steps.Cancel();
}
```

#### 4. Дополнительные методы

**`processor.ExecutionTask`** — таска текущего выполнения. Awaить снаружи чтобы дождаться завершения всей цепочки. При отмене завершается с `OperationCanceledException`.

**`processor.Cancel()`** — отменяет текущую выполняющуюся цепочку без перезапуска.

**Повторный резолв** — вызов `ResolveInstructions` на уже резолвнутом процессоре диспозит предыдущий биндинг и передаёт контроль новому `lifetimeOwner`:

```csharp
_instructionManager.ResolveInstructions(processor, lifetimeOwner: newOwner);
```

---

### Модуль Conditions

#### 1. Зачем

Условия — булевы предикаты с подпиской на изменения. Решают задачу реактивного отображения и поведения: показать/скрыть UI, активировать/деактивировать объект, включить/выключить механику — в зависимости от состояния игры. Условия комбинируются через `All`, `Any`, `None`, `Not`.

#### 2. Как использовать

Два варианта: **inline** и **с хендлером**.

**Inline** — подходит для простых условий, которые опрашиваются напрямую через `IsMet()`.

**С хендлером** — обязателен когда нужна **реактивная подписка на изменения**. Хендлер предоставляет lifecycle-хуки `OnFirstListenerAdded` / `OnLastListenerRemoved`: первый вызывается когда появляется первый подписчик, второй — когда уходит последний. Внутри хуков подписывайся на события источника данных и вызывай `NotifyChanged()` при изменении. Без хендлера изменения не будут нотифицироваться — только прямой опрос.

> **Рекомендация.** Если условие должно реагировать на события (изменение здоровья, смена состояния, таймер) — всегда используй хендлер. Inline подходит только для условий, которые опрашиваются вручную.

##### Inline

```csharp
[Serializable]
[StableRefCategory("Time")]
public class IsNight : Condition
{
    public override bool IsMet() => DayCycle.Current == TimeOfDay.Night;
}
```

##### С хендлером

```csharp
// Данные — лежат в конфиге, сериализуются
[Serializable]
[StableRefCategory("Health")]
public class HealthBelow : ConditionData<HealthBelowHandler>
{
    [Range(0, 1)] public float Threshold;
}

// Хендлер — с подпиской на изменения
public class HealthBelowHandler : ConditionHandler<HealthBelow>
{
    private readonly PlayerHealth _health = ServiceLocator.Get<PlayerHealth>();
    // С Zenject: [Inject] private readonly PlayerHealth _health;

    // Вызывается когда появляется первый подписчик — подписываемся на источник
    protected override void OnFirstListenerAdded()
    {
        _health.OnChanged += NotifyChanged;
    }

    // Вызывается когда уходит последний подписчик — освобождаем подписку
    protected override void OnLastListenerRemoved()
    {
        _health.OnChanged -= NotifyChanged;
    }

    public override bool IsMet() => _health.Ratio < Data.Threshold;
}
```

`NotifyChanged()` — метод базового класса. Вызывай его когда состояние условия изменилось, чтобы все подписчики получили уведомление.

#### 3. Пример использования

Резолв один раз, затем подписка — колбэк вызывается сразу с текущим состоянием и далее при каждом изменении:

```csharp
public class UIHealthWarning : MonoBehaviour
{
    [SerializeField] private ConditionProcessor conditions;

    private readonly IConditionManager _conditionManager = ServiceLocator.Get<IConditionManager>();
    // С Zenject: [Inject] private readonly IConditionManager _conditionManager;

    private void Awake()
    {
        // Резолв один раз — хендлеры привязаны, диспоз при уничтожении объекта
        _conditionManager.ResolveConditions(conditions, lifetimeOwner: this);

        // Подписка напрямую на процессоре
        conditions.Subscribe(isMet => gameObject.SetActive(isMet));
    }

}
```

Прямая проверка без подписки:

```csharp
if (_conditions.IsMet())
{
    // выполнить действие
}
```

#### 4. Дополнительные методы

**`processor.Subscribe(Action<bool> onChanged)`** — регистрирует коллбек, который вызывается сразу с текущим результатом `IsMet()`, а затем при каждом изменении любого условия. Поддерживается несколько подписчиков одновременно.

**`processor.Unsubscribe(Action<bool> onChanged)`** — удаляет конкретный коллбек. Слушатели условий снимаются автоматически когда отписывается последний подписчик.

**`processor.UnsubscribeAll()`** — удаляет всех подписчиков и слушателей разом. Используется менеджером при диспозе биндинга.

**`processor.IsMet()`** — прямая проверка всех условий без подписки. Возвращает `true` если список пуст. Все условия должны быть выполнены (аналог `All`).

**Повторный резолв** — вызов `ResolveConditions` на уже резолвнутом процессоре диспозит предыдущий биндинг (отвязывает хендлеры и вызывает `processor.UnsubscribeAll()`) и передаёт контроль новому `lifetimeOwner`:

```csharp
_conditionManager.ResolveConditions(processor, lifetimeOwner: newOwner);
```

**Композитные условия** — `All`, `Any`, `None`, `Not` комбинируют условия и вкладываются друг в друга:

```
All
├── HealthBelow (0.5)
├── IsNight
└── Not
    └── HasItem (key)
```

Добавляются в инспекторе как обычные условия. Слушатели на композите срабатывают один раз на каждое изменение внутри, независимо от количества внешних подписчиков.

---

### Модуль Extensions

#### 1. Зачем

Экстеншены — носители значений. Позволяют добавлять произвольные значения (кулдаун, максимальное количество, радиус, иконку, префаб) к любому конфигу или компоненту без изменения его класса. Значения можно задать прямо в инспекторе или получать в рантайме через хендлер (загрузить спрайт из asset-менеджера по id, найти значение в БД, или просто получить в рантайме из менеджера).

#### 2. Как использовать

Выбор по двум осям: **inline vs с хендлером** (нужны инжектируемые зависимости?) и **sync vs async** (значение готово сразу или загружается?). Читаешь через `GetValue()` (sync) или `await GetValueAsync(ct)` (async) на том экстеншене, что достал.

- **Inline** — значение прямо на data-классе. Sync-база `Extension<T>` (`T GetValue()`), async-база `AsyncExtension<T>` (`Task<T> GetValueAsync(ct)`).
- **С хендлером** — данные и логика разделены; хендлер создаётся фабрикой и пулится. Sync-пара `ExtensionData` + `ExtensionHandler`, async-пара `AsyncExtensionData` + `AsyncExtensionHandler`.

Async-экстеншены (inline или с хендлером) нужно зарезолвить через `IExtensionManager` перед запросом значения.

> **Отмена.** Диспоз биндинга или уничтожение `lifetimeOwner` отменяет незавершённый `GetValueAsync` — `await` бросит `OperationCanceledException`. Прокидывай токен во все `await` внутри хендлера, иначе загрузка доработает до конца прежде чем остановится.

> **Конкурентность.** У каждого экстеншена свой экземпляр хендлера, поэтому вызывать `GetValueAsync` у разных экстеншенов одновременно безопасно. Вызов у *одного* экстеншена запускает по загрузке на каждый вызов — дедупликации и кэша нет — и безопасен только если хендлер не хранит per-call состояние в полях. Закешируй `Task<TValue>` внутри хендлера, если хочешь одну загрузку на всех. В Unity `await` обычно продолжается на главном потоке, так что это кооперативное чередование, а не настоящий параллелизм.

> **Примечание.** `TryGetExtension` возвращает первый найденный и логирует warning если их несколько — используй `GetExtensions<T>` чтобы перечислить все.

##### Inline — sync

```csharp
[Serializable]
[StableRefCategory("Limits")]
public class MaxCount : Extension<int>
{
    [SerializeField] private int value;

    public override int GetValue() => value;
}
```

##### Inline — async

```csharp
[Serializable]
[StableRefCategory("Remote")]
public class RemoteFlag : AsyncExtension<bool>
{
    public string Key;

    public override async Task<bool> GetValueAsync(CancellationToken cancellationToken = default)
        => await RemoteConfig.GetBoolAsync(Key, cancellationToken);
}
```

##### С хендлером — sync (DI, без await)

```csharp
[Serializable]
[StableRefCategory("Assets")]
public class IconById : ExtensionData<Sprite, IconByIdHandler>
{
    public string Id;
}

public class IconByIdHandler : ExtensionHandler<IconById, Sprite>
{
    private readonly IIconRegistry _icons = ServiceLocator.Get<IIconRegistry>();
    // С Zenject: [Inject] private readonly IIconRegistry _icons;

    public override Sprite GetValue() => _icons.Find(Data.Id);
}
```

##### С хендлером — async (DI + await)

```csharp
[Serializable]
[StableRefCategory("Assets")]
public class SpriteById : AsyncExtensionData<Sprite, SpriteByIdHandler>
{
    public string Id;
}

public class SpriteByIdHandler : AsyncExtensionHandler<SpriteById, Sprite>
{
    private readonly IAssetManager _assets = ServiceLocator.Get<IAssetManager>();
    // С Zenject: [Inject] private readonly IAssetManager _assets;

    public override async Task<Sprite> GetValueAsync(CancellationToken cancellationToken = default)
        => await _assets.LoadSpriteAsync(Data.Id, cancellationToken);
}
```

#### 3. Пример использования

Inline — читается напрямую:

```csharp
// Implicit conversion to T поддерживается
int max = item.Extensions.TryGetExtension(out MaxCount ext) ? ext : int.MaxValue;
```

С хендлером — зарезолвить один раз, затем awaить значение по запросу. Свой токен линкуется с resolve-токеном:

```csharp
_extensionManager.ResolveExtensions(extensions, lifetimeOwner: this);

if (extensions.TryGetExtension(out SpriteById icon))
    image.sprite = await icon.GetValueAsync(cancellationToken);
```

#### 4. Дополнительные методы

**`GetValueAsync(ct)`** — есть у async-экстеншенов (`AsyncExtension<T>` и `AsyncExtensionData`). Для хендлерных требует предварительного `ResolveExtensions`; до байнда логирует ошибку и возвращает `default`. Диспоз биндинга отменяет незавершённый запрос (токен сработает, только если хендлер прокидывает его в свои `await`). Синхронные экстеншены читаются через `GetValue()`.

**`GetExtensions<T>()`** — перечисляет все экстеншены указанного типа:

```csharp
foreach (var tag in config.Extensions.GetExtensions<Tag>())
    Debug.Log(tag.GetValue());
```

---

## Инспектор

Встроенные процессоры (`ModificationProcessor<T>`, `InstructionProcessor`, `ConditionProcessor`, `ExtensionProcessor`) уже отдают типизированный дропдаун — кладёшь процессор в конфиг / компонент и он сразу работает.

Чтобы сгруппировать свои типы под подменю в дропдауне, навесь `[StableRefCategory("Path/Submenu")]`:

```csharp
[Serializable]
[StableRefCategory("Inventory/Item")]
public class MaxCount : Extension<int> { ... }
```

<p align="center">
  <img src="Documentation~/inspector.gif" alt="Добавление Extension через типизированный дропдаун" width="580">
</p>

Если нужен полиморфный список вне встроенных процессоров — используй `StableRefList<T>` напрямую, это тот же тип что используют процессоры внутри.

---

## Контракт лайфтайма

### Как работает биндинг

`ResolveModifications`, `ResolveInstructions`, `ResolveConditions`, `ResolveExtensions` — каждый из этих методов внутри менеджера делает следующее:

1. **Резолвит хендлеры** — берёт из пула или создаёт через фабрику, привязывает к data-объектам.
2. **Возвращает `IDisposable`** — биндинг, при диспозе которого хендлеры возвращаются в пул и всё очищается.

### lifetimeOwner

Параметр обязателен — передай объект-владелец и менеджер задиспозит биндинг автоматически при его уничтожении. Если лайфтайм не нужен и ты контролируешь диспоз сам — передай `null` явно:

```csharp
// Авто-диспоз при уничтожении объекта — IDisposable хранить не нужно
_manager.ResolveInstructions(processor, lifetimeOwner: this);

// Ручной контроль — null явно, IDisposable держишь и диспозишь сам
_binding = _manager.ResolveInstructions(processor, lifetimeOwner: null);

// Комбинированный — авто-диспоз при уничтожении И досрочный ручной диспоз при необходимости
_binding = _manager.ResolveInstructions(processor, lifetimeOwner: this);
// ...
_binding.Dispose(); // безопасно вызвать досрочно; no-op если уже задиспожен через lifetimeOwner
```

### Правила

- Повторный вызов любого `Resolve*` на том же процессоре **автоматически диспозит предыдущий биндинг** — следить за этим не нужно.
- `Dispose()` **идемпотентен** — безопасно вызывать несколько раз или в любом порядке. Исключения при очистке логируются, не пробрасываются.
- Вызов `Apply()`, `IsMet()` или методов подписки на `*Data`-объекте **вне активного биндинга** (до резолва или после диспоза) — no-op: методы возвращают `false` или ничего не делают.

### Хуки жизненного цикла биндинга

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

---

## Утилиты в комплекте

**[StableRef](https://github.com/SST-Systems/StableRef)** — сериализуемая обёртка для полиморфных ссылок поверх `[SerializeReference]`. Выживает при переименовании классов. Редакторные инструменты: поиск по типу, find-usages, отчёт о сломанных ссылках.

**[Pooling](https://github.com/SST-Systems/Pooling)** — лёгкий generic-пул (`Pool<T>`, `MultiPool<TKey, TValue>`). Используется менеджерами для пулинга хендлеров. Нет зависимостей от Unity.

---

## Лицензия

Распространяется под [MIT License](LICENSE.md). Свободно для использования в личных и коммерческих проектах.

Автор — **Egor Shesterikov**.