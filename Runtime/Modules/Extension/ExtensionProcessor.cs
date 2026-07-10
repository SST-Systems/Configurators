using System;
using System.Collections.Generic;
using UnityEngine;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Holds the configured extensions and offers typed lookups over them. Handler-based extensions must be
    /// resolved through <see cref="IExtensionManager"/>; async extensions must be resolved before their value is
    /// requested.
    /// </summary>
    [Serializable]
    public class ExtensionProcessor : IConfiguratorProcessor
    {
        /// <summary>The configured extensions available for lookup.</summary>
        public StableRefList<IExtension> Extensions;

        /// <summary>
        /// Finds the first extension assignable to <typeparamref name="TExtension"/>. Logs a warning if more than
        /// one matches and returns the first.
        /// </summary>
        /// <typeparam name="TExtension">The extension type to look for.</typeparam>
        /// <param name="extension">The first matching extension, or <c>default</c> when none matches.</param>
        /// <returns><c>true</c> if at least one extension matched; otherwise <c>false</c>.</returns>
        public bool TryGetExtension<TExtension>(out TExtension extension) where TExtension : IExtension
        {
            extension = default;

            if (Extensions == null || Extensions.Count == 0)
                return false;

            int matches = 0;

            foreach (var stableRef in Extensions)
            {
                if (stableRef?.Value is TExtension typed)
                {
                    if (matches == 0)
                        extension = typed;

                    matches++;
                }
            }

            if (matches > 1)
            {
                Debug.LogWarning($"[ExtensionProcessor] {matches} extensions of type " +
                                 $"{typeof(TExtension).Name} found; first one returned. " +
                                 "Use GetExtensions to enumerate all matches.");
            }

            return matches > 0;
        }

        /// <summary>Enumerates all extensions assignable to <typeparamref name="TExtension"/>.</summary>
        /// <typeparam name="TExtension">The extension type to look for.</typeparam>
        /// <returns>A lazy sequence of every matching extension.</returns>
        public IEnumerable<TExtension> GetExtensions<TExtension>() where TExtension : IExtension
        {
            if (Extensions == null)
                yield break;

            foreach (var stableRef in Extensions)
                if (stableRef?.Value is TExtension typed)
                    yield return typed;
        }
    }
}