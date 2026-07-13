using System;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Handler-based modification — the serialized data half. Holds only the tint range;
    /// the logic lives in RandomTintHandler so the handler can be pooled/DI-injected.
    /// </summary>
    [Serializable]
    [StableRefCategory("Appearance")]
    public class RandomTint : ModificationData<Shape, RandomTintHandler>
    {
        public Color From = Color.white;
        public Color To = Color.gray;
    }

    /// <summary>
    /// Handler-based modification — the logic half. Reads its RandomTint data via Data
    /// and applies it to the Shape context.
    /// </summary>
    public class RandomTintHandler : ModificationHandler<RandomTint, Shape>
    {
        public override void Apply(Shape context)
            // Pick a random color between From and To (lerp factor 0..1) each time it runs.
            => context.SetColor(Color.Lerp(Data.From, Data.To, UnityEngine.Random.value));
    }
}