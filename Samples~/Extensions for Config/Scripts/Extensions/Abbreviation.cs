using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Inline extension: a short currency abbreviation (e.g. "GLD") held inline.</summary>
    [Serializable]
    [StableRefCategory("Currency")]
    public class Abbreviation : Extension<string>
    {
        [SerializeField] private string value;

        public override string GetValue() => value;
    }
}