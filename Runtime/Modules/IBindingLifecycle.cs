namespace SST.Configurators
{
    /// <summary>
    /// Optional, opt-in contract a configurator implements to take control of its own binding lifetime. When a
    /// configurator (an entry in a processor's serialized list) implements this interface, its manager calls
    /// <see cref="OnResolve"/> as the binding is resolved and <see cref="OnRelease"/> when that binding is
    /// disposed - on a repeat resolve of the same processor or when the lifetime owner is destroyed. Use it to set
    /// up resolve-scoped state such as event subscriptions in <see cref="OnResolve"/> and tear it down in
    /// <see cref="OnRelease"/>. Configurators that do not implement this interface take part in no lifecycle
    /// callbacks. Because the calls are keyed to the binding, they are balanced: every <see cref="OnResolve"/> is
    /// paired with exactly one <see cref="OnRelease"/>.
    /// </summary>
    public interface IBindingLifecycle
    {
        /// <summary>
        /// Called once as the configurator's binding is resolved, after any pooled handler has been bound. Set up
        /// resolve-scoped state here, for example subscribing to events.
        /// </summary>
        void OnResolve();

        /// <summary>
        /// Called once when the configurator's binding is disposed - on a repeat resolve of the same processor or
        /// when the lifetime owner is destroyed. Release anything acquired in <see cref="OnResolve"/> here, for
        /// example unsubscribing from events.
        /// </summary>
        void OnRelease();
    }
}
