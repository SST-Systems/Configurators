using System;
using System.Collections.Generic;

namespace SST.Configurators
{
    /// <summary>
    /// Base class for conditions built from child conditions. Manages forwarding of change notifications: it
    /// subscribes to its children only while it has external listeners and unsubscribes when the last one leaves.
    /// </summary>
    [Serializable]
    public abstract class CompositeCondition : ICompositeCondition
    {
        private Action _onChanged;

        private int _externalListenerCount;
        private bool _innerSubscribed;

        /// <summary>Evaluates the composite from its child conditions.</summary>
        /// <returns><c>true</c> if the composite is currently satisfied.</returns>
        public abstract bool IsMet();

        /// <summary>Returns the child conditions this composite combines.</summary>
        /// <returns>The nested conditions; entries may be <c>null</c>.</returns>
        public abstract IEnumerable<ICondition> GetConditions();

        /// <summary>
        /// Adds a change listener, subscribing to all child conditions on the first external listener.
        /// </summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged)
        {
            if (onChanged == null)
                return;

            _onChanged += onChanged;
            _externalListenerCount++;

            if (_innerSubscribed)
                return;
            
            _innerSubscribed = true;

            foreach (var cond in GetConditions())
                cond?.AddListener(OnInnerChanged);
        }

        /// <summary>
        /// Removes a change listener, unsubscribing from all child conditions when the last external listener leaves.
        /// </summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged)
        {
            if (onChanged == null)
                return;

            _onChanged -= onChanged;

            if (_externalListenerCount <= 0)
                return;

            _externalListenerCount--;

            if (_externalListenerCount > 0)
                return;

            if (!_innerSubscribed)
                return;

            _innerSubscribed = false;

            foreach (var cond in GetConditions())
                cond?.RemoveListener(OnInnerChanged);
        }

        private void OnInnerChanged() => _onChanged?.Invoke();
    }

    /// <summary>
    /// Base class for context-aware conditions built from child conditions over the same
    /// <typeparamref name="TContext"/>. Manages forwarding of change notifications: it subscribes to its children
    /// only while it has external listeners and unsubscribes when the last one leaves. For the context-free variant
    /// see <see cref="CompositeCondition"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    [Serializable]
    public abstract class CompositeCondition<TContext> : ICompositeCondition<TContext>
    {
        private Action _onChanged;

        private int _externalListenerCount;
        private bool _innerSubscribed;

        /// <summary>Evaluates the composite from its child conditions.</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> if the composite is currently satisfied for <paramref name="context"/>.</returns>
        public abstract bool IsMet(TContext context);

        /// <summary>Returns the child conditions this composite combines.</summary>
        /// <returns>The nested conditions; entries may be <c>null</c>.</returns>
        public abstract IEnumerable<ICondition<TContext>> GetConditions();

        /// <summary>
        /// Adds a change listener, subscribing to all child conditions on the first external listener.
        /// </summary>
        /// <param name="onChanged">The callback to add.</param>
        public void AddListener(Action onChanged)
        {
            if (onChanged == null)
                return;

            _onChanged += onChanged;
            _externalListenerCount++;

            if (_innerSubscribed)
                return;

            _innerSubscribed = true;

            foreach (var cond in GetConditions())
                cond?.AddListener(OnInnerChanged);
        }

        /// <summary>
        /// Removes a change listener, unsubscribing from all child conditions when the last external listener leaves.
        /// </summary>
        /// <param name="onChanged">The callback to remove.</param>
        public void RemoveListener(Action onChanged)
        {
            if (onChanged == null)
                return;

            _onChanged -= onChanged;

            if (_externalListenerCount <= 0)
                return;

            _externalListenerCount--;

            if (_externalListenerCount > 0)
                return;

            if (!_innerSubscribed)
                return;

            _innerSubscribed = false;

            foreach (var cond in GetConditions())
                cond?.RemoveListener(OnInnerChanged);
        }

        private void OnInnerChanged() => _onChanged?.Invoke();
    }
}