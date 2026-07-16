# Zenject for Configurators

**English** | [Русский](README.ru.md)

This sample shows how to wire Configurators into a **Zenject** DI container instead of the tiny `ServiceLocator` +
`Bootstrap` pattern the other samples use. The payoff is twofold: the four managers become container singletons you
can `[Inject]` anywhere, and — the important part — your handlers are created *by the container*. That means a
handler can `[Inject]` its own dependencies (an audio bus, a save service, an analytics client) instead of reaching
out to a static locator.

The whole hook-up is just two small pieces: a factory that asks Zenject to instantiate handlers, and an installer
that binds that factory together with the managers. Drop the installer into a Scene (or Project) Context and
everything Configurators-related is resolved through DI.

## What's inside

- `ZenjectHandlerFactory` — implements `IHandlerFactory` by delegating to Zenject's `IInstantiator`, so every
  handler is built through the container and gets full injection.
- `ConfiguratorsZenjectInstaller` — a `MonoInstaller` that binds `IHandlerFactory` → `ZenjectHandlerFactory` and the
  four managers (`IInstructionManager`, `IModificationManager`, `IConditionManager`, `IExtensionManager`) as
  singletons.

## How to use

1. Make sure **Zenject/Extenject** is in your project (the asmdef references `Zenject`).
2. Add a **SceneContext** (or ProjectContext) to your scene.
3. Add `ConfiguratorsZenjectInstaller` to the context's **Mono Installers** list.
4. Done — injected code can now `[Inject]` any of the managers, and every handler your configurators use is
   instantiated by the container, so it can `[Inject]` its own dependencies.

> Requires the **Zenject/Extenject** package. This installer replaces the `Bootstrap` + `ServiceLocator` wiring from
> the other samples — pick one approach per scene, not both.
