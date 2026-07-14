using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Inline sync modification: renames the spawned Shape's GameObject.
    /// Grouped under the "Identity" category in the inspector Add dropdown.
    /// </summary>
    [Serializable]
    [StableRefCategory("Identity")]
    public class SetName : Modification<Shape>
    {
        [SerializeField] private string objectName = "Shape";

        public override void Apply(Shape context) => context.gameObject.name = objectName;
    }
}