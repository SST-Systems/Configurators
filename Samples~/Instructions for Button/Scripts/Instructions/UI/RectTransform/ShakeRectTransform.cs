using System;
using System.Threading;
using System.Threading.Tasks;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Handler-based async instruction (data half): shakes a RectTransform horizontally with decaying
    /// magnitude. Serialized fields live here; the animation logic lives in the paired handler.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/RectTransform")]
    public class ShakeRectTransform : AsyncInstructionData<ShakeRectTransformHandler>
    {
        public RectTransform RectTransform;
        public AnimationSettings AnimSettings;

        // When true the list waits for the shake to finish before the next step; otherwise fire-and-forget.
        [Space]
        public bool WaitForCompletion;

        /// <summary>Inspector-editable settings for the shake animation.</summary>
        [Serializable]
        public class AnimationSettings
        {
            public float Duration = 0.25f;
            public float Magnitude = 15f;
            public float Frequency = 50f;
        }
    }

    /// <summary>
    /// Handler half for <see cref="ShakeRectTransform"/>: runs the shake.
    /// Split from the data so the handler can be pooled / DI-injected by the InstructionManager.
    /// </summary>
    public class ShakeRectTransformHandler : AsyncInstructionHandler<ShakeRectTransform>
    {
        public override Task Apply(CancellationToken cancellationToken = default)
        {
            if (!Data.RectTransform)
                return Task.CompletedTask;

            Vector2 origin = Data.RectTransform.anchoredPosition;
            Task shake = ShakeAsync(origin, cancellationToken);
            // Return the running task only when we should block later steps; else start it and report done.
            return Data.WaitForCompletion ? shake : Task.CompletedTask;
        }

        private async Task ShakeAsync(Vector2 origin, CancellationToken token)
        {
            float elapsed = 0f;

            try
            {
                while (elapsed < Data.AnimSettings.Duration)
                {
                    token.ThrowIfCancellationRequested();

                    float damping = 1f - (elapsed / Data.AnimSettings.Duration);
                    float xOffset = Mathf.Sin(Time.time * Data.AnimSettings.Frequency) * (Data.AnimSettings.Magnitude * damping);

                    Data.RectTransform.anchoredPosition = origin + new Vector2(xOffset, 0f);

                    elapsed += Time.deltaTime;
                    await Task.Yield();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the shake is cancelled; the finally block restores the original position.
            }
            finally
            {
                // Always snap back to where we started, whether we finished, were cancelled, or errored.
                if (Data.RectTransform)
                    Data.RectTransform.anchoredPosition = origin;
            }
        }
    }
}