using System;
using System.Threading;
using System.Threading.Tasks;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Inline async modification (grow-in tween): scales the Shape from zero up to
    /// targetScale over 'duration'. Awaited by the spawner; cancellation is tied to owner lifetime.
    /// </summary>
    [Serializable]
    [StableRefCategory("Transform")]
    public class GrowIn : AsyncModification<Shape>
    {
        [SerializeField] private Vector3 targetScale = Vector3.one;
        [SerializeField] private float duration = 0.35f;

        public override async Task Apply(Shape context, CancellationToken cancellationToken = default)
        {
            Transform t = context.transform;
            t.localScale = Vector3.zero; // Start invisible so the tween can grow it in.

            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Bail out early (throws) if the owner was destroyed mid-tween.
                cancellationToken.ThrowIfCancellationRequested();

                float k = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                t.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, k);

                elapsed += Time.deltaTime;
                await Task.Yield();
            }

            t.localScale = targetScale; // Snap to exact target to avoid rounding drift.
        }
    }
}