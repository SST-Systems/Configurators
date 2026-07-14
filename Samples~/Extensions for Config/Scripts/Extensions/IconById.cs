using System;
using System.Threading;
using System.Threading.Tasks;
using SST.StableRef;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Async handler-based extension: stores only the sprite id; the paired handler does the fetching.</summary>
    [Serializable]
    [StableRefCategory("Currency")]
    public class IconById : AsyncExtensionData<Sprite, IconByIdHandler>
    {
        public string Id;
    }

    /// <summary>Handler for IconById: resolves a Sprite from the SpriteRegistry by id, asynchronously.
    /// Splitting logic from data lets the handler be pooled / DI-injected.</summary>
    public class IconByIdHandler : AsyncExtensionHandler<IconById, Sprite>
    {
        private readonly SpriteRegistry _registry = ServiceLocator.Get<SpriteRegistry>();

        public override async Task<Sprite> GetValueAsync(CancellationToken cancellationToken = default)
        {
            // Simulated async load; forward the token so lifetime/cancellation is honoured.
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            return _registry.Find(Data.Id);
        }
    }
}