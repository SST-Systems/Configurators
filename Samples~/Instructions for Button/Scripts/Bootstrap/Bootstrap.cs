using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Startup component: registers the managers this sample relies on before anything uses them.
    /// Drop it on a scene object so the InstructionManager exists when buttons resolve their instructions.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(new InstructionManager());
        }
    }
}