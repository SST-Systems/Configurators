using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Scene entry point: registers the ExtensionManager so views can resolve extensions.</summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(new ExtensionManager());
        }
    }
}