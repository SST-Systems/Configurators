using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Inline extension: a human-readable display name held directly on the extension.</summary>
    [Serializable]
    [StableRefCategory("Currency")]
    public class DisplayName : Extension<string>
    {
        [SerializeField] private string value;

        public override string GetValue() => value;
    }
}