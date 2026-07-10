using System;
using System.Threading;
using UnityEngine;
using SST.Pooling;

namespace SST.Configurators
{
    /// <summary>
    /// Default <see cref="IModificationManager"/>. Pools <see cref="IModificationHandler"/> instances and binds them
    /// to the serialized data of a <see cref="ModificationProcessor{TContext}"/>.
    /// </summary>
    public class ModificationManager : ConfiguratorManagerBase, IModificationManager
    {
        private readonly MultiPool<Type, IModificationHandler> _handlerPool = new();

        /// <summary>Initializes the manager with the factory used to create handlers.</summary>
        /// <param name="factory">Handler factory; when <c>null</c>, an <see cref="ActivatorHandlerFactory"/> is used.</param>
        public ModificationManager(IHandlerFactory factory = null) : base(factory) { }

        /// <inheritdoc/>
        public IDisposable ResolveModifications<TContext>(ModificationProcessor<TContext> processor, Component lifetimeOwner)
            => Bind(processor, lifetimeOwner, nameof(ResolveModifications), disposable =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                processor.SetResolveCancellation(cancellationTokenSource.Token);

                var modifications = processor.Modifications;

                if (modifications != null)
                    foreach (var stableRef in modifications)
                    {
                        var value = stableRef?.Value;

                        if (value is IHandlerBinder binder)
                            BindHandler(_handlerPool, binder);

                        ResolveLifecycle(disposable, value);
                    }

                RegisterCancellation(disposable, cancellationTokenSource, () => Unregister(processor));
            });

        private void Unregister<TContext>(ModificationProcessor<TContext> processor)
        {
            ActiveBindings.Remove(processor);

            var modifications = processor.Modifications;

            if (modifications == null)
                return;

            foreach (var stableRef in modifications)
                if (stableRef?.Value is IHandlerBinder binder)
                    ReleaseHandler(_handlerPool, binder);
        }
    }
}