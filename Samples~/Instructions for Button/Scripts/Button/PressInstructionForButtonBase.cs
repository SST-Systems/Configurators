using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Base for "press"-style per-event components (pointer down/up). Turns raw pointer and keyboard
    /// Submit input into a single debounced press begin/end pair, so subclasses just override
    /// OnPressBegan/OnPressEnded.
    /// </summary>
    public abstract class PressInstructionForButtonBase : InstructionForButtonBase,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private InputAction _submitAction;
        private bool _isPressed;

        protected void OnDisable()
        {
            // force: true so a press that was in progress ends even if the button is no longer interactable.
            UnbindSubmitAction();
            EndPress(force: true);
        }

        public void OnPointerDown(PointerEventData eventData) => BeginPress();
        public void OnPointerUp(PointerEventData eventData) => EndPress();

        public void OnSelect(BaseEventData eventData) => BindSubmitAction();

        public void OnDeselect(BaseEventData eventData)
        {
            UnbindSubmitAction();
            EndPress(force: true);
        }

        protected abstract void OnPressBegan();

        protected abstract void OnPressEnded();

        private void BeginPress()
        {
            if (_isPressed)
                return;

            if (!button.IsInteractable())
                return;

            _isPressed = true;
            OnPressBegan();
        }

        private void EndPress(bool force = false)
        {
            if (!_isPressed)
                return;

            _isPressed = false;

            if (!force && !button.IsInteractable())
                return;

            OnPressEnded();
        }

        // While selected, listen to the Input System's Submit action so keyboard/gamepad triggers a press too.
        private void BindSubmitAction()
        {
            if (_submitAction != null)
                return;

            if (EventSystem.current == null)
                return;

            if (EventSystem.current.currentInputModule is not InputSystemUIInputModule uiModule)
                return;

            var submitReference = uiModule.submit;

            if (submitReference == null)
                return;

            var action = submitReference.action;

            if (action == null)
                return;

            _submitAction = action;
            _submitAction.started += OnSubmitStarted;
            _submitAction.canceled += OnSubmitCanceled;
        }

        private void UnbindSubmitAction()
        {
            if (_submitAction == null)
                return;

            _submitAction.started -= OnSubmitStarted;
            _submitAction.canceled -= OnSubmitCanceled;
            _submitAction = null;
        }

        private void OnSubmitStarted(InputAction.CallbackContext context) => BeginPress();

        private void OnSubmitCanceled(InputAction.CallbackContext context) => EndPress();
    }
}