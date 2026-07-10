using System;
using System.Collections.Generic;
using UnityEngine;

namespace SST.Configurators
{
    /// <summary>A binding handle that runs its registered release actions in reverse order exactly once on dispose.</summary>
    internal sealed class ProcessorDisposable : IDisposable
    {
        private readonly List<Action> _releaseActions = new();
        private bool _disposed;

        internal void Register(Action release)
        {
            if (release == null)
                return;

            if (_disposed)
            {
                try { release(); }
                catch (Exception e) { Debug.LogException(e); }
                return;
            }

            _releaseActions.Add(release);
        }

        internal void DisposeInternal()
        {
            if (_disposed)
                return;

            ExecuteDispose();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                Debug.LogWarning("[Configurators] Attempted to dispose an already-disposed or replaced binding. " +
                                 "If this binding was replaced by a repeat Resolve, discard the old IDisposable reference.");
                return;
            }

            ExecuteDispose();
        }

        private void ExecuteDispose()
        {
            _disposed = true;

            for (int i = _releaseActions.Count - 1; i >= 0; i--)
            {
                try { _releaseActions[i]?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
            }

            _releaseActions.Clear();
        }
    }
}