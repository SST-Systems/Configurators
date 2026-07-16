# Configurators

## 3.1.0 - 16.07.2026

Context-aware conditions — a generic sibling for every Condition type, alongside the existing context-free ones in the same files.

- Added `ICondition<TContext>` / `Condition<TContext>`, `ConditionData<TContext, THandler>`, `IConditionHandler<TContext>` / `ConditionHandler<TData, TContext>`, `ICompositeCondition<TContext>` / `CompositeCondition<TContext>`, composites `All<TContext>` / `Any<TContext>` / `None<TContext>` / `Not<TContext>`, and `ConditionProcessor<TContext>`. The context flows into `IsMet(TContext context)`, mirroring the Modifications module.
- `IConditionManager` / `ConditionManager` gained a `ResolveConditions<TContext>(ConditionProcessor<TContext>, Component)` overload, backed by a pooled `IContextConditionHandler` marker; composites are walked recursively with the same cycle guard as the context-free path.
- `ConditionProcessor<TContext>` evaluates via `IsMet(TContext)` and keeps a per-subscriber context in `Subscribe(TContext, Action<bool>)`, so a single processor can drive several contexts at once. The reactive change signal stays context-free.

## 3.0.1 - 14.07.2026

Samples overhaul — clearer, self-contained examples. No runtime API changes.

- Renamed samples for clarity: Configurable Button → **Instructions for Button**, Object Configurator → **Modifications for Object**, Reactive Visibility → **Conditions for Visibility**, Currency View → **Extensions for Config**, Zenject Integration → **Zenject for Configurators** — folders, namespaces and assembly definitions updated to match.
- **Modifications for Object**: spawns 2D UI Images instead of 3D objects, spawning automatically every second; `Shape` now wraps a UI `Image`.
- **Conditions for Visibility**: reworked around UI toggles (removed the timer/tick mechanism); `StateConditionController` → `StatefulWidgetController`; added a ready-made two-state `DefaultStatefulWidget` (Active / Deactive); sample instructions are now `ChangeImageColor` and `SetTextLegacy`.
- Made each sample's configurators distinct, so importing all samples at once introduces no duplicate behaviours.
- Renamed serialized fields for clarity: `ExtensionProcessor` on `CurrencyConfig`, `modificationProcessor` on `ShapeSpawner`.
- Docs: added English/Russian READMEs for the Zenject sample, rewrote the sample READMEs in plainer language, refreshed the sample table and fixed links in the main README, and tidied in-code comments across all samples.
- Fixed the sample links in the main README that broke on spaces in `Samples~/...` paths (now URL-encoded).
- Aligned the Modifications module example with the Quick Start — shared `Unit` context and `EnemySpawner`, so the two sections read consistently (no API change).
- Added a "Usage Lifecycle" section with a block diagram (`Documentation~/lifecycle.svg`, `lifecycle.ru.svg`) to both READMEs, showing how to pick a module and the shared resolve → use → cleanup flow.

## 3.0.0 - 10.07.2026

First public release under **SST Systems**.

Design gameplay rules in the Unity Inspector — no code. Modifications, Instructions, Conditions, Extensions: serializable, pooled, DI-ready.
