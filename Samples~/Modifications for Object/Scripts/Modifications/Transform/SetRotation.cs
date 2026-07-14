using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Inline sync modification: sets the Shape's rotation from Inspector-authored euler angles.
    /// </summary>
    [Serializable]
    [StableRefCategory("Transform")]
    public class SetRotation : Modification<Shape>
    {
        [SerializeField] private Vector3 eulerAngles;

        public override void Apply(Shape context) => context.transform.rotation = Quaternion.Euler(eulerAngles);
    }
}