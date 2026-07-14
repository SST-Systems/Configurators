using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Base per-event component: binds an Inspector-edited InstructionProcessor to a UGUI Button.
    /// Subclasses hook a specific pointer/button event and call TryApply() to play the list.
    /// </summary>
    public abstract class InstructionForButtonBase : MonoBehaviour
    {
        [SerializeField] protected Button button;
        [SerializeField] protected InstructionProcessor instructionProcessor;
        
        public InstructionProcessor Processor => instructionProcessor;

        protected virtual void Reset()
        {
            button = GetComponent<Button>();
        }

        protected virtual void Awake()
        {
            if (!button)
                button = GetComponent<Button>();
        }

        protected virtual void Start()
        {
            // Ask the manager to wire up handlers (pooling/DI) for the serialized instruction list.
            ServiceLocator.Get<InstructionManager>().ResolveInstructions(instructionProcessor, this);
        }

        public void Cancel()
        {
            instructionProcessor?.Cancel();
        }

        protected void TryApply()
        {
            if (instructionProcessor?.Instructions == null || instructionProcessor.Instructions.Count == 0)
                return;

            instructionProcessor.Apply();
        }
    }
}