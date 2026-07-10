using System;
using System.Threading;
using UnityEngine;
using SST.Pooling;

namespace SST.Configurators
{
    /// <summary>
    /// Default <see cref="IInstructionManager"/>. Pools <see cref="IInstructionHandler"/> instances and binds them
    /// to the serialized data of an <see cref="InstructionProcessor"/>.
    /// </summary>
    public class InstructionManager : ConfiguratorManagerBase, IInstructionManager
    {
        private readonly MultiPool<Type, IInstructionHandler> _handlerPool = new();

        /// <summary>Initializes the manager with the factory used to create handlers.</summary>
        /// <param name="factory">Handler factory; when <c>null</c>, an <see cref="ActivatorHandlerFactory"/> is used.</param>
        public InstructionManager(IHandlerFactory factory = null) : base(factory) { }

        /// <inheritdoc/>
        public IDisposable ResolveInstructions(InstructionProcessor processor, Component lifetimeOwner)
            => Bind(processor, lifetimeOwner, nameof(ResolveInstructions), disposable =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                processor.SetResolveCancellation(cancellationTokenSource.Token);

                var instructions = processor.Instructions;

                if (instructions != null)
                    foreach (var stableRef in instructions)
                    {
                        var value = stableRef?.Value;

                        if (value is IHandlerBinder binder)
                            BindHandler(_handlerPool, binder);

                        ResolveLifecycle(disposable, value);
                    }

                RegisterCancellation(disposable, cancellationTokenSource, () => Unregister(processor));
            });

        private void Unregister(InstructionProcessor processor)
        {
            ActiveBindings.Remove(processor);

            var instructions = processor.Instructions;

            if (instructions == null)
                return;

            foreach (var stableRef in instructions)
                if (stableRef?.Value is IHandlerBinder binder)
                    ReleaseHandler(_handlerPool, binder);
        }
    }
}