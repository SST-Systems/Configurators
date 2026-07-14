using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Inline sync modification: sets the Shape's Image color to a fixed value.
    /// </summary>
    [Serializable]
    [StableRefCategory("Appearance")]
    public class SetColor : Modification<Shape>
    {
        [SerializeField] private Color color = Color.white;

        public override void Apply(Shape context) => context.SetColor(color);
    }
}