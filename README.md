<img src="Documentation~/banner.png" width="900" alt="Configurators">

[![release](https://img.shields.io/github/v/release/SST-Systems/Configurators)](../../releases)
[![release date](https://img.shields.io/github/release-date/SST-Systems/Configurators)](../../releases)
[![last commit](https://img.shields.io/github/last-commit/SST-Systems/Configurators)](../../commits)
[![license](https://img.shields.io/github/license/SST-Systems/Configurators)](LICENSE.md)

**English** | [Русский](README.ru.md)

---

Design gameplay rules in the Inspector — no code changes to add or tweak behaviours. Conditions, effects, modifications, and extensions are all serializable, handler-pooled, and DI-ready.

## Table Of Contents

<details>
<summary>Details</summary>

- [Installation](#installation)
- [Concepts](#concepts)
- [Setup](#setup)
  - [Creating managers](#creating-managers)
  - [Creating the handler factory](#creating-the-handler-factory)
- [Modules](#modules)
  - [Modifications](#modifications-module)
  - [Instructions](#instructions-module)
  - [Conditions](#conditions-module)
  - [Extensions](#extensions-module)
- [Inspector](#inspector)
- [Lifetime Contract](#lifetime-contract)
  - [Binding lifecycle hooks](#binding-lifecycle-hooks)
- [Under the Hood](#under-the-hood)
- [Bundled Utilities](#bundled-utilities)

</details>

---

## Installation

1. **.unitypackage** — [Releases](../../releases)
2. **UPM** — `Window → Package Manager` → `+` → `Add package from git URL`. UPM doesn't resolve git dependencies automatically — add all three:
   - [Pooling](https://github.com/SST-Systems/Pooling): `https://github.com/SST-Systems/Pooling.git`
   - [StableRef](https://github.com/SST-Systems/StableRef): `https://github.com/SST-Systems/StableRef.git`
   - Configurators: `https://github.com/SST-Systems/Configurators.git`

   Append `#tag` to each URL to pin a version.
3. **Manual** — clone or download all three repos, copy to `Assets/`.

Unity 2021.3+

> Optional Zenject integration is included as a sample — import it via `Window → Package Manager` → select Configurators → **Samples** tab.

---

## Concepts

| Term | Meaning |
|---|---|
| **Modification** | A unit of work applied to a `TContext`, run sequentially. Example: "set max HP", "add tag". |
| **Instruction** | A command with no context — targets are inspector references on the data class, run sequentially. Example: `GameObjectSetActive`, `PlaySound`, `WaitForSeconds`. |
| **Condition** | A boolean predicate with change subscription (`AddListener`) and direct query (`IsMet`). |
| **Extension** | A value carrier attached to a config or component, read on demand. Example: cooldown, max count, an icon. |
| **Processor** | Container holding a list of elements for one module. Lives on a config or component. |
| **Handler** | Pooled runtime logic for a data object. Required when you need injectable dependencies. |
| **HandlerFactory** | Controls how handler instances are created. Default is `Activator.CreateInstance`. |

---

## Setup

### Creating managers

Each module has its own manager. Create the ones you need and keep them for the lifetime of your scene or project:

```csharp
IInstructionManager instructionManager = new InstructionManager();
IModificationManager modificationManager = new ModificationManager();
IConditionManager conditionManager = new ConditionManager();
IExtensionManager extensionManager = new ExtensionManager();
```

Each component injects only the interface it actually needs.

### Creating the handler factory

Handlers are pooled runtime objects — they're created once, reused, and returned to the pool on dispose. Because of this they can't be instantiated with `new` by a standard DI container: the container doesn't know when to create them or how many to produce. The factory bridges that gap: it's the single place that knows how to construct a handler, so it can delegate to the DI container and get all dependencies injected automatically.

Managers delegate handler instantiation to `IHandlerFactory`:

```csharp
public interface IHandlerFactory
{
    object Create(Type handlerType);
}
```

**Default — `ActivatorHandlerFactory`** — creates handlers via `Activator.CreateInstance`. Works when handlers have no dependencies (or resolve them manually via a service locator).

**Custom factory** — implement `IHandlerFactory` to have the DI container create handlers and inject their dependencies:

```csharp
public class ZenjectHandlerFactory : IHandlerFactory
{
    [Inject] private readonly IInstantiator _instantiator;

    public object Create(Type handlerType) => _instantiator.Instantiate(handlerType);
}
```

With this in place, any handler can declare its own constructor or `[Inject]` fields and receive dependencies exactly like any other class — the factory takes care of the rest.

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

Ready-made integration in [`Samples~/Zenject/`](Samples~/Zenject/).

---

## Modules

The primary storage unit is the **Processor** — it holds and configures the list of elements directly in the Inspector. Each module has its own processor type (`ModificationProcessor<T>`, `InstructionProcessor`, `ConditionProcessor`, `ExtensionProcessor`) that you place on a config or component.

Once declared, a processor must be resolved through its manager before use — this binds handlers to data objects and prepares the runtime objects. Lifetime is controlled by `lifetimeOwner` (auto-dispose on destroy) or managed manually via the returned `IDisposable`.

```csharp
// 1. Create the manager once (scene/project lifetime)
IModificationManager manager = new ModificationManager();

// 2. Resolve before use — binds handlers
manager.ResolveModifications(processor, lifetimeOwner: this);

// 3. Run after resolve — Apply returns a Task; await it if you need to wait for async steps
await processor.Apply(context);
```

> **About the examples.** Code samples below don't use any DI container. `ServiceLocator.Get<T>()` is a stand-in — how you actually obtain your managers (manual instantiation, Zenject, VContainer, or any other approach) is entirely up to you. `ActorUnit`, `PlayerHealth`, `IGameFactory`, `ISkinService` and similar types are project-specific placeholders; substitute them with your own.

### Modifications Module

#### 1. What it solves

Modifications apply effects to a context — a unit, an entity, any data object. Example use cases: configure actor stats on spawn, apply item effects to a player, alter level parameters. All logic is described in the Inspector with no code changes. Modifications run **sequentially**; each step can be synchronous or, when it needs to wait for something (load an asset, fetch a value), asynchronous — the next one starts only after the current finishes.

#### 2. How to use

Pick along two axes: **inline vs handler-based** (do you need injected dependencies?) and **sync vs async** (do you need to `await`?). Sync is the default and the lightest to write — reach for async only when a step must wait.

- **Inline** — logic lives on the data class. Sync base `Modification<TContext>` (`void Apply`), async base `AsyncModification<TContext>` (`Task Apply`).
- **Handler-based** — data and behaviour are split; the handler is created by the factory and pooled. Sync pair `ModificationData` + `ModificationHandler`, async pair `AsyncModificationData` + `AsyncModificationHandler`.

The processor runs the list in order — sync entries inline, async entries awaited. If every entry is synchronous, the whole run completes synchronously.

> **Recommendation.** Default to sync (`Modification` / `ModificationData`); switch to the async variants only when a step genuinely awaits.

> **Cancellation.** Disposing the binding or destroying the `lifetimeOwner` cancels in-flight async runs. You can also pass your own token to `Apply`. Forward the token into every `await` inside your async modifications (and call `cancellationToken.ThrowIfCancellationRequested()` in loops), otherwise the current step runs to completion before it stops.

> **Concurrency.** The processor keeps no per-run state, so one processor can be applied to many objects at once — e.g. `await Task.WhenAll(units.Select(u => modifications.Apply(u)))`. Each context's chain runs independently (and in order within itself). Safe as long as handlers keep no per-call state in fields — the context flows as a parameter, not stored.

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

##### Handler-based — sync (DI, no await)

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
    // With Zenject: [Inject] private readonly IGameFactory _factory;

    public override void Apply(ActorUnit context) => _factory.Spawn(Data.Prefab, context.transform.position);
}
```

##### Handler-based — async (DI + await)

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
    // With Zenject: [Inject] private readonly ISkinService _skins;

    public override async Task Apply(ActorUnit context, CancellationToken cancellationToken = default)
    {
        var skin = await _skins.LoadAsync(Data.SkinId, cancellationToken);
        context.SetSkin(skin);
    }
}
```

#### 3. Usage example

```csharp
public class ActorSpawner : MonoBehaviour
{
    [SerializeField] private ModificationProcessor<ActorUnit> modifications;

    private readonly IModificationManager _modificationManager = ServiceLocator.Get<IModificationManager>();
    // With Zenject: [Inject] private readonly IModificationManager _modificationManager;

    private void Awake()
    {
        // Resolve once — handlers are bound, chain cancelled when this is destroyed
        _modificationManager.ResolveModifications(modifications, lifetimeOwner: this);
    }

    // Configure a freshly spawned actor — awaits every modification in order
    public async Task Configure(ActorUnit actor)
    {
        try { await modifications.Apply(actor); }
        catch (OperationCanceledException) { /* actor or spawner destroyed mid-configure */ }
    }
}
```

With manual control (`lifetimeOwner: null`):

```csharp
private IDisposable _binding;

private void Setup(ModificationProcessor<SomeContext> processor)
{
    _binding = _modificationManager.ResolveModifications(processor, lifetimeOwner: null);
}

private void Cleanup()
{
    _binding?.Dispose(); // handlers returned to pool
}
```

#### 4. Additional methods

**Re-resolving** — calling `ResolveModifications` on an already-resolved processor automatically disposes the previous binding and creates a new one. Use this to transfer control to a new `lifetimeOwner`:

```csharp
_modificationManager.ResolveModifications(processor, lifetimeOwner: newOwner);
```

---

### Instructions Module

#### 1. What it solves

Instructions are commands with no context. Targets are stored as inspector references on the data class. Instructions run sequentially — each one starts only after the previous finishes. A step can be synchronous or asynchronous (delays, awaits, cancellation). Typical use cases: level event sequences, animation chains, tutorial steps, delays between actions.

#### 2. How to use

Pick along two axes: **inline vs handler-based** (do you need injected dependencies?) and **sync vs async** (do you need to `await`?). Sync is the default.

- **Inline** — logic on the data class. Sync base `Instruction` (`void Apply()`), async base `AsyncInstruction` (`Task Apply(ct)`).
- **Handler-based** — data and behaviour split; the handler is created by the factory and pooled. Sync pair `InstructionData` + `InstructionHandler`, async pair `AsyncInstructionData` + `AsyncInstructionHandler`.

The processor runs the list in order — sync entries inline, async entries awaited.

> **Recommendation.** Default to sync (`Instruction` / `InstructionData`); use the async variants when a step must wait.

> **Cancellation.** When the binding is disposed or `lifetimeOwner` is destroyed, the running chain is cancelled; a repeat `Apply` cancels the previous run. In your async instructions, forward the token into every `await` (`Task.Delay(ms, cancellationToken)`, `UniTask.Delay(...)`, etc.) and call `cancellationToken.ThrowIfCancellationRequested()` in loops — otherwise the current step runs to completion before it stops.

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
// Delay — token passed to Task.Delay, cancellation works immediately
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

// Loop with per-iteration cancellation check
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

##### Handler-based — sync (DI, no await)

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
    // With Zenject: [Inject] private readonly IAudioService _audioService;

    public override void Apply() => _audioService.SetMasterVolume(Data.Volume);
}
```

##### Handler-based — async (DI + await)

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
    // With Zenject: [Inject] private readonly IAudioService _audioService;

    public override async Task Apply(CancellationToken cancellationToken = default)
    {
        await _audioService.PlayAndWait(Data.Clip, Data.Volume, cancellationToken);
    }
}
```

#### 3. Usage example

```csharp
public class TutorialController : MonoBehaviour
{
    [SerializeField] private InstructionProcessor steps;

    private readonly IInstructionManager _instructionManager = ServiceLocator.Get<IInstructionManager>();
    // With Zenject: [Inject] private readonly IInstructionManager _instructionManager;

    private void Awake()
    {
        // Resolve once — handlers are bound, chain cancelled when this is destroyed
        _instructionManager.ResolveInstructions(steps, lifetimeOwner: this);
    }

    // Start the chain — repeated calls automatically cancel the previous run
    public void StartTutorial()
    {
        steps.Apply();
    }

    // Start and await completion of the entire chain
    public async Task StartAndAwait()
    {
        steps.Apply();

        try
        {
            await steps.ExecutionTask;
            Debug.Log("All instructions completed");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Chain was cancelled");
        }
    }

    // Early cancellation without restart
    public void StopTutorial() => steps.Cancel();
}
```

#### 4. Additional methods

**`processor.ExecutionTask`** — the Task for the current run. Await it to observe when the entire chain finishes. Completes with `OperationCanceledException` on cancellation.

**`processor.Cancel()`** — cancels the currently running chain without restarting it.

**Re-resolving** — calling `ResolveInstructions` on an already-resolved processor disposes the previous binding and transfers control to a new `lifetimeOwner`:

```csharp
_instructionManager.ResolveInstructions(processor, lifetimeOwner: newOwner);
```

---

### Conditions Module

#### 1. What it solves

Conditions are boolean predicates with change subscriptions. They handle reactive display and behaviour: show/hide UI, activate/deactivate objects, toggle mechanics — based on game state. Conditions can be composed with `All`, `Any`, `None`, `Not`.

#### 2. How to use

Two flavours: **inline** and **handler-based**.

**Inline** — for simple conditions polled directly via `IsMet()`.

**Handler-based** — required when you need **reactive change subscriptions**. The handler provides lifecycle hooks `OnFirstListenerAdded` / `OnLastListenerRemoved`: the first is called when the first subscriber appears, the second when the last subscriber leaves. Subscribe to your data source events inside these hooks and call `NotifyChanged()` when state flips. Without a handler, change notifications won't work — only direct polling.

> **Recommendation.** If a condition must react to events (health change, state machine transition, timer) — always use a handler. Inline is only suitable for conditions polled manually.

##### Inline

```csharp
[Serializable]
[StableRefCategory("Time")]
public class IsNight : Condition
{
    public override bool IsMet() => DayCycle.Current == TimeOfDay.Night;
}
```

##### Handler-based

```csharp
// Data — lives in the config, serialized
[Serializable]
[StableRefCategory("Health")]
public class HealthBelow : ConditionData<HealthBelowHandler>
{
    [Range(0, 1)] public float Threshold;
}

// Handler — with change subscription
public class HealthBelowHandler : ConditionHandler<HealthBelow>
{
    private readonly PlayerHealth _health = ServiceLocator.Get<PlayerHealth>();
    // With Zenject: [Inject] private readonly PlayerHealth _health;

    // Called when the first listener subscribes — subscribe to the data source
    protected override void OnFirstListenerAdded()
    {
        _health.OnChanged += NotifyChanged;
    }

    // Called when the last listener unsubscribes — release the subscription
    protected override void OnLastListenerRemoved()
    {
        _health.OnChanged -= NotifyChanged;
    }

    public override bool IsMet() => _health.Ratio < Data.Threshold;
}
```

`NotifyChanged()` is a base class method. Call it whenever the condition state changes to notify all subscribers.

#### 3. Usage example

Resolve once, then subscribe — the callback fires immediately with the current state and again on every change:

```csharp
public class UIHealthWarning : MonoBehaviour
{
    [SerializeField] private ConditionProcessor conditions;

    private readonly IConditionManager _conditionManager = ServiceLocator.Get<IConditionManager>();
    // With Zenject: [Inject] private readonly IConditionManager _conditionManager;

    private void Awake()
    {
        // Resolve once — handlers bound, auto-disposed when this is destroyed
        _conditionManager.ResolveConditions(conditions, lifetimeOwner: this);

        // Subscribe directly on the processor
        conditions.Subscribe(isMet => gameObject.SetActive(isMet));
    }

}
```

Direct poll without subscription:

```csharp
if (_conditions.IsMet())
{
    // perform action
}
```

#### 4. Additional methods

**`processor.Subscribe(Action<bool> onChanged)`** — registers a callback that fires immediately with the current `IsMet()` result, then again every time any condition changes. Multiple subscribers are supported.

**`processor.Unsubscribe(Action<bool> onChanged)`** — removes a specific callback. Condition listeners are released automatically when the last subscriber unsubscribes.

**`processor.UnsubscribeAll()`** — removes all subscribers and listeners at once. Used internally by the manager on binding dispose.

**`processor.IsMet()`** — directly evaluates all conditions. Returns `true` if the list is empty. Every condition must be met (equivalent to `All`).

**Re-resolving** — calling `ResolveConditions` on an already-resolved processor disposes the previous binding (which unbinds handlers and calls `processor.UnsubscribeAll()`) and transfers control to a new `lifetimeOwner`:

```csharp
_conditionManager.ResolveConditions(processor, lifetimeOwner: newOwner);
```

**Composite conditions** — `All`, `Any`, `None`, `Not` combine conditions and nest freely:

```
All
├── HealthBelow (0.5)
├── IsNight
└── Not
    └── HasItem (key)
```

Added in the Inspector like regular conditions. Listeners fire once per inner change, regardless of how many external listeners are attached.

---

### Extensions Module

#### 1. What it solves

Extensions are value carriers. They let you attach arbitrary values — cooldown, max count, radius, an icon, a prefab — to any config or component without modifying its class. Values can be authored directly in the Inspector, or produced at runtime by a handler (load a sprite from an asset manager by id, look up a value in a database, or simply fetch it at runtime from a manager).

#### 2. How to use

Pick along two axes: **inline vs handler-based** (do you need injected dependencies?) and **sync vs async** (is the value ready immediately, or loaded?). Read it by calling `GetValue()` (sync) or `await GetValueAsync(ct)` (async) on the extension you fetched.

- **Inline** — the value lives on the data class. Sync base `Extension<T>` (`T GetValue()`), async base `AsyncExtension<T>` (`Task<T> GetValueAsync(ct)`).
- **Handler-based** — data and behaviour split; the handler is created by the factory and pooled. Sync pair `ExtensionData` + `ExtensionHandler`, async pair `AsyncExtensionData` + `AsyncExtensionHandler`.

Async extensions (inline or handler-based) must be resolved through `IExtensionManager` before their value is requested.

> **Cancellation.** Disposing the binding or destroying the `lifetimeOwner` cancels an in-flight `GetValueAsync` — the `await` throws `OperationCanceledException`. Forward the token into every `await` inside your handler, otherwise the load runs to completion before it stops.

> **Concurrency.** Each extension owns its own handler instance, so calling `GetValueAsync` on different extensions concurrently is safe. Calling it on the *same* extension runs one load per call — there's no dedup or caching — and is only safe when the handler keeps no per-call state in fields. Cache the `Task<TValue>` inside the handler if you want callers to share a single load. In Unity `await` usually resumes on the main thread, so this is cooperative interleaving rather than true parallelism.

> **Note.** `TryGetExtension` returns the first match and logs a warning if there are several — use `GetExtensions<T>` to enumerate them all.

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

##### Handler-based — sync (DI, no await)

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
    // With Zenject: [Inject] private readonly IIconRegistry _icons;

    public override Sprite GetValue() => _icons.Find(Data.Id);
}
```

##### Handler-based — async (DI + await)

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
    // With Zenject: [Inject] private readonly IAssetManager _assets;

    public override async Task<Sprite> GetValueAsync(CancellationToken cancellationToken = default)
        => await _assets.LoadSpriteAsync(Data.Id, cancellationToken);
}
```

#### 3. Usage example

Inline — read directly:

```csharp
// Implicit conversion to T is supported
int max = item.Extensions.TryGetExtension(out MaxCount ext) ? ext : int.MaxValue;
```

Handler-based — resolve once, then await the value on demand. Your own token is linked with the resolve-scoped one:

```csharp
_extensionManager.ResolveExtensions(extensions, lifetimeOwner: this);

if (extensions.TryGetExtension(out SpriteById icon))
    image.sprite = await icon.GetValueAsync(cancellationToken);
```

#### 4. Additional methods

**`GetValueAsync(ct)`** — exposed by async extensions (`AsyncExtension<T>` and `AsyncExtensionData`). For handler-based ones it requires a prior `ResolveExtensions`; called before binding it logs an error and returns `default`. Disposing the binding cancels an in-flight request (the token is honoured only if the handler forwards it into its own `await`s). Sync extensions read synchronously via `GetValue()` instead.

**`GetExtensions<T>()`** — enumerates all extensions of a given type:

```csharp
foreach (var tag in config.Extensions.GetExtensions<Tag>())
    Debug.Log(tag.GetValue());
```

---

## Inspector

The built-in processors (`ModificationProcessor<T>`, `InstructionProcessor`, `ConditionProcessor`, `ExtensionProcessor`) already expose a typed dropdown — drop one on a config or component and it just works.

To group your types under a submenu in that dropdown, decorate the class with `[StableRefCategory("Path/Submenu")]`:

```csharp
[Serializable]
[StableRefCategory("Inventory/Item")]
public class MaxCount : Extension<int> { ... }
```

<p align="center">
  <img src="Documentation~/inspector.gif" alt="Adding an Extension via the typed dropdown" width="580">
</p>

If you need a polymorphic list outside the built-in processors, use `StableRefList<T>` directly — that's the same type the processors use internally.

---

## Lifetime Contract

### How bindings work

`ResolveModifications`, `ResolveInstructions`, `ResolveConditions`, `ResolveExtensions` — each of these methods internally:

1. **Resolves handlers** — takes from pool or creates via factory, binds to data objects.
2. **Returns `IDisposable`** — a binding that, when disposed, returns handlers to the pool and cleans up everything.

### lifetimeOwner

The parameter is required — pass an owner object and the manager disposes the binding automatically when it is destroyed. If you don't need auto-dispose and control the lifetime yourself — pass `null` explicitly:

```csharp
// Auto-dispose when this is destroyed — no need to store IDisposable
_manager.ResolveInstructions(processor, lifetimeOwner: this);

// Manual control — pass null explicitly, store and dispose IDisposable yourself
_binding = _manager.ResolveInstructions(processor, lifetimeOwner: null);

// Combined — auto-dispose on destroy AND early manual dispose when needed
_binding = _manager.ResolveInstructions(processor, lifetimeOwner: this);
// ...
_binding.Dispose(); // safe to call early; no-op if already disposed by lifetimeOwner
```

### Rules

- Calling any `Resolve*` again on the same processor **automatically disposes the previous binding** — no manual cleanup needed.
- `Dispose()` is **idempotent** — safe to call multiple times or in any order. Exceptions during cleanup are logged, not thrown.
- Calling `Apply()`, `IsMet()`, or listener methods on a `*Data` object **outside an active binding** (before resolve or after dispose) is a no-op — methods return `false` or do nothing.

### Binding lifecycle hooks

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

---

## Bundled Utilities

**[StableRef](https://github.com/SST-Systems/StableRef)** — serializable polymorphic reference wrapper built on `[SerializeReference]`. Survives class renames by storing a stable type ID. Editor tools: searchable selector, find-usages, broken-reference report.

**[Pooling](https://github.com/SST-Systems/Pooling)** — lightweight generic pool (`Pool<T>`, `MultiPool<TKey, TValue>`). Used by managers to pool handlers. No Unity dependencies.

---

## License

Distributed under the [MIT License](LICENSE.md). Free for personal and commercial use.

Author — **Egor Shesterikov**.