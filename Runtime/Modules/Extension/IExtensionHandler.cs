namespace SST.Configurators
{
    /// <summary>
    /// Marker interface used as the pool key for extension handlers. Handlers are pooled and not reset on return,
    /// so they must keep no per-call state.
    /// </summary>
    public interface IExtensionHandler { }

    /// <summary>
    /// A synchronous extension handler that receives injected data and produces a value on demand.
    /// </summary>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    public interface ISyncExtensionHandler<TValue> : IExtensionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The extension data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Produces the extension's value.</summary>
        /// <returns>The current value.</returns>
        TValue GetValue();
    }
}