using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Inline extension: a Sprite referenced directly (no lookup needed).</summary>
    [Serializable]
    [StableRefCategory("Currency")]
    public class Icon : Extension<Sprite>
    {
        [SerializeField] private Sprite value;

        public override Sprite GetValue() => value;
    }
}