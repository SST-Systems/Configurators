# Modifications for Object

**English** | [Русский](README.ru.md)

This sample is about **modifications** — one-shot effects that set up a target object (its *context*). The idea:
describe *how* an object should be configured as a reorderable list in the Inspector, then apply that same list to
any object you like.

Here a spawner drops a new 2D image onto the canvas every second and runs one modification list against each one —
grow it in, give it a colour, add a random tint, rotate it. Every image goes through the exact same list, so
changing how *all* future ones look is just a matter of editing that list — adding, removing, reordering steps. The
spawner code that creates them never changes.

## Preview

<p align="center">
  <img src="../../Documentation~/samples/modifications.gif" alt="Modifications for Object demo" width="800">
</p>

## What's inside

- `Shape` — the context (a wrapper around a UI `Image`) that modifications configure.
- `ShapeSpawner` — spawns an image under a `RectTransform` every second and runs `await modificationProcessor.Apply(shape)` on it.
- Modifications: `SetColor`, `SetName`, `SetRotation` (inline sync), `GrowIn` (inline async),
  `RandomTint` (handler — data and logic split).
- `Bootstrap` registers the `ModificationManager`; `ServiceLocator` is a tiny registry.

## Run it

Open `Scenes/Sample Scene.unity` and press Play. A new shape spawns automatically every second, each one configured
by the same modification list. Select `ShapeSpawner` in the Inspector and reorder or tweak the list (or the spawn
interval) — every future spawn changes with it, no code edits.

> One processor can be applied to many contexts. Changing the list in the Inspector reconfigures every future
> spawn with no code c