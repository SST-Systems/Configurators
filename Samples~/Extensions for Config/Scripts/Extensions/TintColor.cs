using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Inline extension: a label tint colour held inline.</summary>
    [Serializable]
    [StableRefCategory("Currency")]
    public class TintColor : Extension<Color>
    {
        [SerializeField] private Color value = Color.white;

        public override Color GetValue() => value;
    }
}