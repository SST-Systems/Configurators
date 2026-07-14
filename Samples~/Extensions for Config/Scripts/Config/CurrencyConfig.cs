using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>The config — stays tiny (id + amount); all optional facets live in the ExtensionProcessor,
    /// so adding a new facet never changes this class.</summary>
    [CreateAssetMenu(menuName = "Configurators Samples/Currency", fileName = "Currency")]
    public class CurrencyConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private int amount;
        [SerializeField] private ExtensionProcessor extensionProcessor;

        public string Id => id;
        public int Amount => amount;
        public ExtensionProcessor ExtensionProcessor => extensionProcessor;
    }
}