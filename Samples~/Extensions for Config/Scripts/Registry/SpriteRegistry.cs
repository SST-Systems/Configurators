using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators.Samples.ExtensionsForConfig
{
    /// <summary>Minimal id → Sprite lookup — a stand-in for an asset manager / Addressables.</summary>
    public class SpriteRegistry : MonoBehaviour
    {
        /// <summary>One id → Sprite pairing, edited as a list in the inspector.</summary>
        [Serializable]
        private struct Entry
        {
            public string Id;
            public Sprite Sprite;
        }

        [SerializeField] private List<Entry> entries = new();

        private void Awake() => ServiceLocator.Register(this);

        private void OnDestroy() => ServiceLocator.Unregister<SpriteRegistry>();

        public Sprite Find(string id)
        {
            foreach (Entry entry in entries)
                if (entry.Id == id)
                    return entry.Sprite;

            return null;
        }
    }
}