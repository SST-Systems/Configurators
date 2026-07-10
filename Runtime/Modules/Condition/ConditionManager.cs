using System;
using System.Collections.Generic;
using UnityEngine;
using SST.Pooling;

namespace SST.Configurators
{
    /// <summary>
    /// Default <see cref="IConditionManager"/>. Pools <see cref="IConditionHandler"/> instances and binds them to
    /// the serialized data of a <see cref="ConditionProcessor"/>, walking composites recursively and guarding
    /// against cyclic references.
    /// </summary>
    public class ConditionManager : ConfiguratorManagerBase, IConditionManager
    {
        private readonly MultiPool<Type, IConditionHandler> _handlerPool = new();

        /// <summary>Initializes the manager with the factory used to create handlers.</summary>
        /// <param name="factory">Handler factory; when <c>null</c>, an <see cref="ActivatorHandlerFactory"/> is used.</param>
        public ConditionManager(IHandlerFactory factory = null) : base(factory) { }

        /// <inheritdoc/>
        public IDisposable ResolveConditions(ConditionProcessor processor, Component lifetimeOwner)
            => Bind(processor, lifetimeOwner, nameof(ResolveConditions), disposable =>
            {
                var visited = UnityEngine.Pool.HashSetPool<ICondition>.Get();
                var currentPath = UnityEngine.Pool.HashSetPool<ICondition>.Get();

                try
                {
                    var conditions = processor.Conditions;
                    if (conditions != null)
                        foreach (var stableRef in conditions)
                            BindConditionRecursive(stableRef?.Value, visited, currentPath);
                }
                finally
                {
                    UnityEngine.Pool.HashSetPool<ICondition>.Release(currentPath);
                    UnityEngine.Pool.HashSetPool<ICondition>.Release(visited);
                }

                disposable.Register(() => Unregister(processor));
                disposable.Register(processor.UnsubscribeAll);
            });

        private void Unregister(ConditionProcessor processor)
        {
            ActiveBindings.Remove(processor);

            var conditions = processor.Conditions;

            if (conditions == null)
                return;

            var visited = UnityEngine.Pool.HashSetPool<ICondition>.Get();

            try
            {
                foreach (var stableRef in conditions)
                    ReleaseConditionRecursive(stableRef?.Value, visited);
            }
            finally
            {
                UnityEngine.Pool.HashSetPool<ICondition>.Release(visited);
            }
        }

        private void BindConditionRecursive(ICondition condition, HashSet<ICondition> visited, HashSet<ICondition> currentPath)
        {
            if (condition == null)
                return;

            if (currentPath.Contains(condition))
            {
                Debug.LogError($"[ConditionManager] Cycle detected while resolving conditions: " +
                               $"{condition.GetType().Name} forms a circular reference.");
                return;
            }

            if (!visited.Add(condition))
                return;

            if (condition is IHandlerBinder binder)
                BindHandler(_handlerPool, binder);

            if (condition is IBindingLifecycle lifecycle)
                lifecycle.OnResolve();

            if (condition is ICompositeCondition composite)
            {
                currentPath.Add(condition);
                foreach (var inner in composite.GetConditions())
                    BindConditionRecursive(inner, visited, currentPath);
                currentPath.Remove(condition);
            }
        }

        private void ReleaseConditionRecursive(ICondition condition, HashSet<ICondition> visited)
        {
            if (condition == null)
                return;

            if (!visited.Add(condition))
                return;

            if (condition is IHandlerBinder binder)
                ReleaseHandler(_handlerPool, binder);

            if (condition is IBindingLifecycle lifecycle)
                lifecycle.OnRelease();

            if (condition is ICompositeCondition composite)
                foreach (var inner in composite.GetConditions())
                    ReleaseConditionRecursive(inner, visited);
        }
    }
}