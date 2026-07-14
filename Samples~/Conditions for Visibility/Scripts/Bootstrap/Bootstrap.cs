using UnityEngine;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Scene entry point: registers the condition and instruction managers in the <see cref="ServiceLocator"/>
    /// so widgets and handlers can find them at runtime. This is the sample's lightweight stand-in for DI setup.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(new ConditionManager());
            ServiceLocator.Register(new InstructionManager());
        }
    }
}