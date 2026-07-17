using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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

    /// <summary>
    /// Evaluates a set of context-aware conditions as a logical AND and notifies subscribers reactively when the
    /// combined result changes. The context flows in per evaluation: <see cref="IsMet"/> takes it as a parameter,
    /// and each reactive subscriber supplies its own context, so one processor can drive several contexts at once.
    /// Handler-based entries must first be resolved through <see cref="IConditionManager"/>. For the context-free
    /// variant see <see cref="ConditionProcessor"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the conditions are evaluated against.</typeparam>
    [Serializable]
    public class ConditionProcessor<TContext> : IConfiguratorProcessor
    {
        /// <summary>The configured conditions, combined with logical AND by <see cref="IsMet"/>.</summary>
        public StableRefList<ICondition<TContext>> Conditions;

        private readonly List<Subscription> _subscribers = new();
        private bool _innerSubscribed;

        /// <summary>
        /// Subscribes to combined-result changes for the given context, subscribing to the underlying conditions on
        /// the first listener and immediately invoking the callback with the current result. The callback is later
        /// re-invoked with <see cref="IsMet"/> evaluated against the same <paramref name="context"/> on every change.
        /// </summary>
        /// <param name="context">The context this subscriber's result is evaluated against.</param>
        /// <param name="onChanged">Callback receiving the current combined result and each subsequent change.</param>
        public void Subscribe(TContext context, Action<bool> onChanged)
        {
            if (onChanged == null)
                return;

            if (!_innerSubscribed && Conditions is { Count: > 0 })
            {
                foreach (var stableRef in Conditions)
                    stableRef?.Value?.AddListener(OnConditionChanged);

                _innerSubscribed = true;
            }

            _subscribers.Add(new Subscription(context, onChanged));

            try { onChanged.Invoke(IsMet(context)); }
            catch (Exception e) { Debug.LogException(e); }
        }

        /// <summary>
        /// Unsubscribes a callback, unsubscribing from the underlying conditions once the last listener is removed.
        /// Removes every registration made with this <paramref name="onChanged"/> delegate.
        /// </summary>
        /// <param name="onChanged">The callback previously passed to <see cref="Subscribe"/>.</param>
        public void Unsubscribe(Action<bool> onChanged)
        {
            if (onChanged == null)
                return;

            for (var i = _subscribers.Count - 1; i >= 0; i--)
                if (_subscribers[i].OnChanged == onChanged)
                    _subscribers.RemoveAt(i);

            if (_subscribers.Count == 0)
                UnsubscribeInner();
        }

        /// <summary>Removes all subscribers and unsubscribes from the underlying conditions.</summary>
        public void UnsubscribeAll()
        {
            _subscribers.Clear();
            UnsubscribeInner();
        }

        private void UnsubscribeInner()
        {
            if (!_innerSubscribed)
                return;

            if (Conditions is { Count: > 0 })
                foreach (var stableRef in Conditions)
                    stableRef?.Value?.RemoveListener(OnConditionChanged);

            _innerSubscribed = false;
        }

        private void OnConditionChanged()
        {
            if (_subscribers.Count == 0)
                return;

            // Snapshot so callbacks may (un)subscribe during notification without mutating the live list.
            var buffer = ListPool<Subscription>.Get();

            try
            {
                buffer.AddRange(_subscribers);

                foreach (var sub in buffer)
                {
                    try { sub.OnChanged.Invoke(IsMet(sub.Context)); }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }
            finally
            {
                ListPool<Subscription>.Release(buffer);
            }
        }

        /// <summary>
        /// Evaluates all conditions as a logical AND against the given context. An empty or null set is considered met.
        /// </summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> if every condition is met for <paramref name="context"/>, or there are none.</returns>
        public bool IsMet(TContext context)
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (!(stableRef?.Value?.IsMet(context) ?? true))
                    return false;

            return true;
        }

        private readonly struct Subscription
        {
            public readonly TContext Context;
            public readonly Action<bool> OnChanged;

            public Subscription(TContext context, Action<bool> onChanged)
            {
                Context = context;
                OnChanged = onChanged;
            }
        }
    }
}