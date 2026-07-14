# Configurators — Advanced

**English** | [Русский](ADVANCED.ru.md) · [← back to README](../README.md)

This document is the deep-dive split out of the README: fine-grained lifetime control and the internals of the managers. You don't need it to get started — read it when you want to predict edge cases or take lifetime management into your own hands.

- [Binding lifecycle hooks](#binding-lifecycle-hooks)
- [Under the Hood](#under-the-hood)

---

## Binding lifecycle hooks

Any configurator — an entry in a processor's list, whether a modification, instruction, condition, or extension — can take control of its own binding lifetime by implementing `IBindingLifecycle`. When it does, the manager calls `OnResolve()` as the binding is resolved and `OnRelease()` when that binding is disposed (on a repeat resolve of the same processor, or when the `lifetimeOwner` is destroyed). It's fully opt-in: configurators that don't implement the interface take part in no lifecycle callbacks.

Use it for resolve-scoped state that must be set up and torn down together with the binding — most commonly event subscriptions:

```csharp
[Serializable]
public class PlaySound : Instruction, IBindingLifecycle
{
    [Inject] private IAudioBus _audioBus;

    void IBindingLifecycle.OnResolve() => _audioBus.MuteChanged += OnMuteChanged;
    void IBindingLifecycle.OnRelease() => _audioBus.MuteChanged -= OnMuteChanged;

    public override void Apply() { /* play the sound */ }

    private void OnMuteChanged(bool muted) { /* react */ }
}
```

Guarantees:

- **Paired 1:1** — every `OnResolve()` is followed by exactly one `OnRelease()`. A repeat resolve disposes the old binding (`OnRelease`) before building the new one (`OnResolve`).
- **Ordering** — `OnResolve()` runs *after* any pooled handler has been bound, so a handler-based configurator can rely on its handler already being attached.
- **Composites** — conditions nested inside `All`/`Any`/`None`/`Not` receive the hooks too; each unique configurator is called once, even when shared or referenced through composites.
- **Keep subscription state on the configurator, not on a pooled handler** — handlers are shared and reused across resolves, so a subscription stored there would leak.

`OnResolve()` is your code and may throw; the exception propagates out of the `Resolve*` call, just like a handler binding failure (it is not swallowed). Because `OnRelease` is registered only after `OnResolve` returns, a throwing `OnResolve` never leaves a dangling `OnRelease`.

---

## Under the Hood

This section describes exactly what happens inside a manager on each call. Knowing this helps predict edge cases and use the API with confidence.

### Handler Pool

Each manager owns a `MultiPool<Type, IXxxHandler>` — a pool keyed by handler type. On the first request for a type, the factory's `Create(type)` is called once and a factory delegate is registered; all subsequent requests return a pooled instance. On dispose, handlers are unbound from data and returned to the pool. Handlers are **not** reset on return — if your handler accumulates state, reset it in `SetData` or `Apply`.

### Active Bindings Registry

Every manager holds a `Dictionary<object, IDisposable> ActiveBindings` mapping each processor to its current binding. When any `Resolve*` is called again on the same processor, the dictionary finds the previous binding and silently disposes it before creating a new one.

### ProcessorDisposable — LIFO Cleanup

The `IDisposable` returned from `Resolve*` is a `ProcessorDisposable` — an ordered list of cleanup actions. On `Dispose()` it executes them **in reverse (LIFO)** order. Each action is wrapped in a try/catch so one failure doesn't block the rest.

For `ResolveInstructions` the registration order is:

1. `cancellationTokenSource.Dispose()`
2. `binding.Dispose()` — unbinds handlers, removes from ActiveBindings
3. `cancellationTokenSource.Cancel()`

Reversed on dispose → **Cancel → Unbind handlers → Dispose CTS**. Cancellation must propagate into the running task before handlers are released.

`ProcessorDisposable` is idempotent: the first `Dispose()` sets a flag; subsequent calls are no-ops. If `Register()` is called on an already-disposed instance (e.g. race with `lifetimeOwner` destruction), the action fires immediately so nothing leaks.

### ProcessorReleaser — Lifetime Binding

`ProcessorReleaser` is an internal `MonoBehaviour` added to `lifetimeOwner.gameObject` on first use (`TryGetComponent`, otherwise `AddComponent`). It holds a list of `IDisposable` bindings and calls `Dispose()` on each one — in reverse order — inside `OnDestroy`. The `[DisallowMultipleComponent]` attribute ensures only one releaser exists per GameObject; multiple managers on the same owner share the same component.

If the GameObject is already destroyed when `Add()` is called (race condition), the releaser disposes the incoming binding immediately rather than silently dropping it.

### Full Lifecycle of ResolveInstructions

```
ResolveInstructions(processor, lifetimeOwner)   // binds handlers; does NOT run them
│
├─ 1. processor == null? → log error, return null
│
├─ 2. Previous binding for this processor? → Dispose() (Cancel + Unbind + Dispose CTS)
│
├─ 3. new ProcessorDisposable; ActiveBindings[processor] = disposable
│
├─ 4. new CancellationTokenSource cts; processor.SetResolveCancellation(cts.Token)
│
├─ 5. foreach instruction in processor.Instructions
│       instruction is IHandlerBinder?
│         pool.HasFactory(type)? no → RegisterFactory(() => factory.Create(type))
│         pool.Get(type) → pooled instance or freshly created
│         binder.BindHandler(handler) → data stores handler reference
│
├─ 6. Register cleanup (runs LIFO): cts.Dispose → Unregister(processor) → cts.Cancel
│
└─ 7. BindLifetime(disposable, lifetimeOwner)
        TryGetComponent<ProcessorReleaser>, otherwise AddComponent
        releaser.Add(disposable)

// Separate, user-invoked step — NOT part of resolve:
processor.Apply(cancellationToken)
│
├─ cancel/dispose its own previous run, then link the caller token with the resolve token
├─ ExecutionTask = RunAsync(linkedToken)
└─ RunAsync: foreach instruction
       cancellationToken.ThrowIfCancellationRequested()
       instruction is IAsyncInstruction ? await Apply(token) : Apply()
       OperationCanceledException → re-throw (stops the chain)
       other exception            → Debug.LogException (continues)
```

### Dispose Chain

```
combined.Dispose()
  — triggered by: user code, lifetimeOwner.OnDestroy, or repeat ResolveInstructions
│
├─ 1. cancellationTokenSource.Cancel()
│       token becomes cancelled
│       running instruction's Apply() throws OperationCanceledException
│       RunAsync re-throws → ExecutionTask faults with OperationCanceledException
│
├─ 2. binding.Dispose() → Unregister(processor)
│       ActiveBindings.Remove(processor)
│       foreach instruction is IHandlerBinder
│           binder.UnbindHandler() → data nulls its handler reference
│           pool.Release(type, handler) → handler returned to pool
│
└─ 3. cancellationTokenSource.Dispose()
        releases the OS wait handle associated with the token
```
