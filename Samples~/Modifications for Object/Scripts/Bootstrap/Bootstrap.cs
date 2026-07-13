using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Scene entry point: registers the ModificationManager so systems like ShapeSpawner can resolve modifications at runtime.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(new ModificationManager());
        }
    }
}