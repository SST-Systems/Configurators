using System;
using System.Threading;
using System.Threading.Tasks;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Handler-based async instruction (data half): animates a RectTransform's local scale over time.
    /// Serialized fields live here; the actual coroutine-like logic lives in the paired handler.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/RectTransform")]
    public class SetScaleRectTransform : AsyncInstructionData<SetScaleRectTransformHandler>
    {
        public RectTransform RectTransform;
        public AnimationSettings AnimSettings;

        // When true the list waits for this scale to finish before running the next step; otherwise fire-and-forget.
        [Space]
        public bool WaitForCompletion;

        /// <summary>Inspector-editable tween settings for the scale animation.</summary>
        [Serializable]
        public class AnimationSettings
        {
            public float Duration = 0.12f;
            public float Scale = 1f;
        }
    }

    /// <summary>
    /// Handler half for <see cref="SetScaleRectTransform"/>: runs the scale tween.
    /// Split from the data so the handler can be pooled / DI-injected by the InstructionManager.
    /// </summary>
    public class SetScaleRectTransformHandler : AsyncInstructionHandler<SetScaleRectTransform>
    {
        public override async Task Apply(CancellationToken cancellationToken = default)
        {
            if (!Data.RectTransform)
                return;

            if (Data.WaitForCompletion)
                await ScaleAsync(cancellationToken);
            else
                _ = ScaleAsync(cancellationToken); // fire-and-forget: let later steps run while this animates.
        }

        private async Task ScaleAsync(CancellationToken token)
        {
            Vector3 from = Data.RectTransform.localScale;
            Vector3 to = Vector3.one * Data.AnimSettings.Scale;
            float elapsed = 0f;

            try
            {
                while (elapsed < Data.AnimSettings.Duration)
                {
                    token.ThrowIfCancellationRequested();

                    float t = Mathf.SmoothStep(0f, 1f, elapsed / Data.AnimSettings.Duration);
                    Data.RectTransform.localScale = Vector3.LerpUnclamped(from, to, t);

                    elapsed += Time.deltaTime;

                    await Task.Yield();
                }

                token.ThrowIfCancellationRequested();

                Data.RectTransform.localScale = to;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected (e.g. CancelInstructionForButton); swallow it and leave scale as-is.
            }
        }
    }
}