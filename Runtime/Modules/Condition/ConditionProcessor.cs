using System;
using UnityEngine;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Evaluates a set of conditions as a logical AND and notifies subscribers reactively when the combined result
    /// changes. Handler-based entries must first be resolved through <see cref="IConditionManager"/>.
    /// </summary>
    [Serializable]
    public class ConditionProcessor : IConfiguratorProcessor
    {
        /// <summary>The configured conditions, combined with logical AND by <see cref="IsMet"/>.</summary>
        public StableRefList<ICondition> Conditions;

        private Action<bool> _onChanged;

        /// <summary>
        /// Subscribes to combined-result changes, subscribing to the underlying conditions on the first listener
        /// and immediately invoking the callback with the current result.
        /// </summary>
        /// <param name="onChanged">Callback receiving the current combined result and each subsequent change.</param>
        public void Subscribe(Action<bool> onChanged)
        {
            if (onChanged == null)
                return;

            if (_onChanged == null && Conditions is { Count: > 0 })
                foreach (var stableRef in Conditions)
                    stableRef?.Value?.AddListener(OnConditionChanged);

            _onChanged += onChanged;

            try { onChanged.Invoke(IsMet()); }
            catch (Exception e) { Debug.LogException(e); }
        }

        /// <summary>
        /// Unsubscribes a callback, unsubscribing from the underlying conditions once the last listener is removed.
        /// </summary>
        /// <param name="onChanged">The callback previously passed to <see cref="Subscribe"/>.</param>
        public void Unsubscribe(Action<bool> onChanged)
        {
            if (onChanged == null)
                return;

            _onChanged -= onChanged;

            if (_onChanged == null && Conditions is { Count: > 0 })
                foreach (var stableRef in Conditions)
                    stableRef?.Value?.RemoveListener(OnConditionChanged);
        }

        /// <summary>Removes all subscribers and unsubscribes from the underlying conditions.</summary>
        public void UnsubscribeAll()
        {
            if (_onChanged == null)
                return;

            if (Conditions is { Count: > 0 })
                foreach (var stableRef in Conditions)
                    stableRef?.Value?.RemoveListener(OnConditionChanged);

            _onChanged = null;
        }

        private void OnConditionChanged()
        {
            try { _onChanged?.Invoke(IsMet()); }
            catch (Exception e) { Debug.LogException(e); }
        }

        /// <summary>Evaluates all conditions as a logical AND. An empty or null set is considered met.</summary>
        /// <returns><c>true</c> if every condition is met, or there are none.</returns>
        public bool IsMet()
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (!(stableRef?.Value?.IsMet() ?? true))
                    return false;

            return true;
        }
    }
}