# Instructions for Button

[English](README.md) | **Русский**

Этот пример про **инструкции (instructions)** — маленькие самодостаточные действия (что-то масштабировать,
потрясти, перекрасить текст, залогировать сообщение), которые ты складываешь в список прямо в инспекторе, без кода.

Здесь у кнопки на каждое событие — клик, наведение/уход, нажатие/отпускание, выбор/снятие — своя такая цепочка.
Когда событие срабатывает, кнопка просто проигрывает список сверху вниз. Шаги бывают мгновенными или растянутыми во
времени (async), а более поздний шаг может даже отменить ещё выполняющийся. Нужна разная реакция на наведение и на
клик? Переставь или замени шаги в инспекторе — сам скрипт кнопки при этом не меняется.

## Превью

<p align="center">
  <img src="../../Documentation~/samples/instructions.gif" alt="Instructions for Button demo" width="800">
</p>

## Что внутри

- `InstructionForButtonBase` / `PressInstructionForButtonBase` — компоненты, привязывающие `InstructionProcessor`
  к событиям `Button` и резолвящие его через менеджер.
- Компоненты по событиям: `Click`, `PointerEnter`/`PointerExit`, `PointerDown`/`PointerUp`, `Select`/`Deselect`.
- Инструкции: `LogMessage`, `SelectSelectable`/`DeselectSelectable`, `SetScaleRectTransform`/`ShakeRectTransform`
  (async), `ChangeTextColor`, `CancelInstructionForButton`.
- `Bootstrap` регистрирует `InstructionManager`; `ServiceLocator` — простой реестр.
- Готовая сцена — `Scenes/Sample Scene.unity`.

## Запуск

Открой `Scenes/Sample Scene.unity` и нажми Play. Наводись/кликай по кнопке — на каждое событие отрабатывает
своя цепочка. Списки инструкций и их порядок настраиваются в инспекторе на компонентах кнопки.

> Требует пакет **Input System** (asmdef ссылается на `Unity.InputSystem`).
