using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using SST.Pooling;

namespace SST.Configurators
{
    /// <summary>
    /// Shared base for the module managers. Provides handler pooling, binding lifecycle, and cancellation
    /// wiring used when resolving a processor's serialized data into pooled handlers.
    /// </summary>
    public abstract class ConfiguratorManagerBase
    {
        private readonly IHandlerFactory _factory;

        /// <summary>
        /// Active bindings keyed by processor. Resolving a processor that already has a binding disposes the
        /// previous one first, guaranteeing a single live binding per processor.
        /// </summary>
        protected readonly Dictionary<object, IDisposable> ActiveBindings = new();

        /// <summary>
        /// Initializes the manager with the factory used to instantiate handlers.
        /// </summary>
        /// <param name="factory">Handler factory; when <c>null</c>, an <see cref="ActivatorHandlerFactory"/> is used.</param>
        protected ConfiguratorManagerBase(IHandlerFactory factory = null)
        {
            _factory = factory ?? new ActivatorHandlerFactory();
        }

        /// <summary>
        /// Disposes a binding without emitting the "already disposed" warning that a normal
        /// <see cref="IDisposable.Dispose"/> would produce, used when a binding is being replaced internally.
        /// </summary>
        /// <param name="disposable">The binding to dispose.</param>
        protected static void DisposeBindingInternal(IDisposable disposable)
        {
            if (disposable is ProcessorDisposable processorDisposable)
                processorDisposable.DisposeInternal();
            else
                disposable.Dispose();
        }

        /// <summary>
        /// Ties a binding to the lifetime of a Unity <paramref name="owner"/> so the binding is disposed
        /// automatically when the owner is destroyed. Attaches a hidden releaser component to the owner if needed.
        /// </summary>
        /// <param name="disposable">The binding to auto-dispose on destroy.</param>
        /// <param name="owner">The component whose destruction should dispose the binding. Ignored when <c>null</c>.</param>
        protected void BindLifetime(IDisposable disposable, Component owner)
        {
            if (disposable == null || owner == null)
                return;

            if (!owner.TryGetComponent(out ProcessorReleaser releaser))
                releaser = owner.gameObject.AddComponent<ProcessorReleaser>();

            releaser.Add(disposable);

            if (disposable is ProcessorDisposable processorDisposable)
                processorDisposable.Register(() => releaser.Remove(disposable));
        }

        /// <summary>
        /// Obtains a pooled handler for the binder's <see cref="IHandlerBinder.HandlerType"/> and binds it to the
        /// serialized data. Logs an error and skips the entry if the handler type is null or instantiation fails.
        /// </summary>
        /// <typeparam name="T">The module's handler interface used as the pool value type.</typeparam>
        /// <param name="pool">The pool the handler is drawn from.</param>
        /// <param name="binder">The serialized data acting as the handler binder.</param>
        protected void BindHandler<T>(MultiPool<Type, T> pool, IHandlerBinder binder) where T : class
        {
            var type = binder.HandlerType;

            if (type == null)
            {
                Debug.LogError($"[{GetType().Name}] {binder.GetType().Name} returned null HandlerType.");
                return;
            }

            T handler;

            try
            {
                handler = GetOrCreate(pool, type);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] Failed to instantiate handler {type.Name}: {ex.Message}");
                throw;
            }

            if (handler == null)
            {
                Debug.LogError($"[{GetType().Name}] Handler {type.Name} resolved to null.");
                return;
            }

            binder.BindHandler(handler);
        }

        /// <summary>
        /// Unbinds the handler currently attached to <paramref name="binder"/> and returns it to the pool.
        /// </summary>
        /// <typeparam name="T">The module's handler interface used as the pool value type.</typeparam>
        /// <param name="pool">The pool the handler is returned to.</param>
        /// <param name="binder">The serialized data whose handler should be released.</param>
        protected void ReleaseHandler<T>(MultiPool<Type, T> pool, IHandlerBinder binder) where T : class
        {
            if (binder.GetHandler() is T handler)
            {
                binder.UnbindHandler();
                pool.Release(binder.HandlerType, handler);
            }
        }

        /// <summary>
        /// Gets a pooled handler of the given <paramref name="type"/>, registering a factory backed by the
        /// configured <see cref="IHandlerFactory"/> on first use.
        /// </summary>
        /// <typeparam name="T">The module's handler interface used as the pool value type.</typeparam>
        /// <param name="pool">The pool to draw from.</param>
        /// <param name="type">The concrete handler type to obtain.</param>
        /// <returns>A pooled or newly created handler instance.</returns>
        protected T GetOrCreate<T>(MultiPool<Type, T> pool, Type type) where T : class
        {
            if (!pool.HasFactory(type))
                pool.RegisterFactory(type, () => (T)_factory.Create(type));

            return pool.Get(type);
        }

        /// <summary>
        /// Core resolve routine: validates the processor, disposes any previous binding for it, creates a fresh
        /// binding, runs the module-specific <paramref name="configure"/> step, and ties it to the lifetime owner.
        /// </summary>
        /// <param name="processor">The processor being resolved; logs an error and returns <c>null</c> when null.</param>
        /// <param name="lifetimeOwner">Optional component that auto-disposes the binding on destroy.</param>
        /// <param name="methodName">Calling method name, used only for diagnostics.</param>
        /// <param name="configure">Module-specific binding setup that registers handlers and cancellation.</param>
        /// <returns>The new binding, or <c>null</c> if the processor was null.</returns>
        private protected IDisposable Bind(object processor, Component lifetimeOwner, string methodName,
            Action<ProcessorDisposable> configure)
        {
            if (processor == null)
            {
                Debug.LogError($"[{GetType().Name}] {methodName} called without a processor (null).");
                return null;
            }

            if (ActiveBindings.TryGetValue(processor, out var previous))
                DisposeBindingInternal(previous);

            var disposable = new ProcessorDisposable();
            ActiveBindings[processor] = disposable;

            configure(disposable);

            BindLifetime(disposable, lifetimeOwner);
            return disposable;
        }

        /// <summary>
        /// Invokes the opt-in lifecycle hooks on a configurator that implements <see cref="IBindingLifecycle"/>:
        /// calls <see cref="IBindingLifecycle.OnResolve"/> now and registers
        /// <see cref="IBindingLifecycle.OnRelease"/> to run when the binding is disposed. No-op for values that
        /// don't implement the interface or are <c>null</c>.
        /// </summary>
        /// <param name="disposable">The binding the release hook is registered on.</param>
        /// <param name="value">The configurator being resolved.</param>
        private protected static void ResolveLifecycle(ProcessorDisposable disposable, object value)
        {
            if (value is not IBindingLifecycle lifecycle)
                return;

            lifecycle.OnResolve();
            disposable.Register(lifecycle.OnRelease);
        }

        /// <summary>
        /// Registers the teardown steps for a binding: cancel the resolve-scoped token source, run the
        /// module's unregister action, and dispose the token source when the binding is disposed.
        /// </summary>
        /// <param name="disposable">The binding to register the teardown actions on.</param>
        /// <param name="cancellationTokenSource">The resolve-scoped token source to cancel and dispose.</param>
        /// <param name="unregister">The module-specific action that releases handlers and forgets the processor.</param>
        private protected void RegisterCancellation(ProcessorDisposable disposable,
            CancellationTokenSource cancellationTokenSource, Action unregister)
        {
            disposable.Register(() => cancellationTokenSource.Dispose());
            disposable.Register(unregister);
            disposable.Register(() => cancellationTokenSource.Cancel());
        }
    }
}