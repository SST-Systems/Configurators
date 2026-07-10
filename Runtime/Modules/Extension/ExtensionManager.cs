using System;
using System.Threading;
using UnityEngine;
using SST.Pooling;

namespace SST.Configurators
{
    /// <summary>
    /// Default <see cref="IExtensionManager"/>. Pools <see cref="IExtensionHandler"/> instances and binds them to
    /// the serialized data of an <see cref="ExtensionProcessor"/>, wiring resolve-scoped cancellation into async
    /// extensions.
    /// </summary>
    public class ExtensionManager : ConfiguratorManagerBase, IExtensionManager
    {
        private readonly MultiPool<Type, IExtensionHandler> _handlerPool = new();

        /// <summary>Initializes the manager with the factory used to create handlers.</summary>
        /// <param name="factory">Handler factory; when <c>null</c>, an <see cref="ActivatorHandlerFactory"/> is used.</param>
        public ExtensionManager(IHandlerFactory factory = null) : base(factory) { }

        /// <inheritdoc/>
        public IDisposable ResolveExtensions(ExtensionProcessor processor, Component lifetimeOwner)
            => Bind(processor, lifetimeOwner, nameof(ResolveExtensions), disposable =>
            {
                var cancellationTokenSource = new CancellationTokenSource();

                var extensions = processor.Extensions;

                if (extensions != null)
                    foreach (var stableRef in extensions)
                    {
                        var value = stableRef?.Value;

                        if (value is IHandlerBinder binder)
                            BindHandler(_handlerPool, binder);

                        if (value is IResolveCancellationBinder cancellable)
                            cancellable.SetResolveCancellation(cancellationTokenSource.Token);

                        ResolveLifecycle(disposable, value);
                    }

                RegisterCancellation(disposable, cancellationTokenSource, () => Unregister(processor));
            });

        private void Unregister(ExtensionProcessor processor)
        {
            ActiveBindings.Remove(processor);

            var extensions = processor.Extensions;

            if (extensions == null)
                return;

            foreach (var stableRef in extensions)
                if (stableRef?.Value is IHandlerBinder binder)
                    ReleaseHandler(_handlerPool, binder);
        }
    }
}