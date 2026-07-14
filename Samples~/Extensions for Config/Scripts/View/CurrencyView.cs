using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>The view: asks TryGetExtension&lt;T&gt; for each facet and renders only the ones present,
    /// so rich and bare currencies both work with the same code.</summary>
    public class CurrencyView : MonoBehaviour
    {
        [SerializeField] private CurrencyConfig config;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;
        [SerializeField] private Text amount;

        private readonly CancellationTokenSource _cts = new();

        private void Start()
        {
            // Resolve the config's extensions (injects handlers) before reading any of them.
            ServiceLocator.Get<ExtensionManager>().ResolveExtensions(config.ExtensionProcessor, this);
            Render();
        }

        private void OnDestroy()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private void Render()
        {
            ExtensionProcessor ext = config.ExtensionProcessor;

            string text = ext.TryGetExtension(out DisplayName displayName) ? (string)displayName : config.Id;

            if (ext.TryGetExtension(out Abbreviation abbreviation))
                text = $"{text} ({(string)abbreviation})";
            
            label.text = text;

            if (ext.TryGetExtension(out TintColor tint))
                label.color = tint;

            amount.text = config.Amount.ToString();

            RenderIcon(ext);
        }

        private async void RenderIcon(ExtensionProcessor ext)
        {
            Sprite sprite = null;

            // Prefer the inline Icon (value held directly); otherwise fetch via the async handler-based IconById.
            if (ext.TryGetExtension(out Icon inlineIcon))
            {
                sprite = inlineIcon;
            }
            else if (ext.TryGetExtension(out IconById asyncIcon))
            {
                try { sprite = await asyncIcon.GetValueAsync(_cts.Token); }
                catch (OperationCanceledException) { return; } // view destroyed mid-load
            }

            if (!icon)
                return;

            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
    }
}