using System;
using System.Collections;
using UnityEngine;

namespace SST.Configurators.Samples.ModificationsForObject
{
    /// <summary>
    /// Spawns a new Shape every interval and applies the SAME modification list to each one.
    /// Editing the list in the Inspector reconfigures every future spawn with no code changes.
    /// </summary>
    public class ShapeSpawner : MonoBehaviour
    {
        [SerializeField] private ModificationProcessor<Shape> modificationProcessor;
        [SerializeField] private Shape prefab;
        [SerializeField] private RectTransform spawnRoot;
        [SerializeField] private Vector2 area = new Vector2(300f, 150f);
        [SerializeField] private float spawnInterval = 1f;

        private void Start()
        {
            // Inject handlers/dependencies into the list; 'this' owns cancellation for async mods.
            ServiceLocator.Get<ModificationManager>().ResolveModifications(modificationProcessor, this);
            StartCoroutine(SpawnLoop());
        }

        // Auto-spawn coroutine: drops one Shape every spawnInterval seconds, forever.
        private IEnumerator SpawnLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(spawnInterval);
            while (true)
            {
                Spawn();
                yield return wait;
            }
        }

        private async void Spawn()
        {
            Shape shape = Instantiate(prefab, spawnRoot);

            // anchoredPosition is 2D UI space, not world space.
            RectTransform rect = (RectTransform)shape.transform;
            rect.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(-area.x, area.x),
                UnityEngine.Random.Range(-area.y, area.y));

            try
            {
                await modificationProcessor.Apply(shape);
            }
            catch (OperationCanceledException)
            {
                // Owner destroyed mid-tween: async modification was cancelled, nothing to do.
            }
        }
    }
}