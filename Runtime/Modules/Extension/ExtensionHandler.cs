namespace SST.Configurators
{
    /// <summary>
    /// Base class for a synchronous extension handler, exposing the injected data to subclasses.
    /// Because handlers are pooled and reused, keep no per-call state outside <see cref="Data"/>.
    /// </summary>
    /// <typeparam name="TData">The serialized data type this handler operates on.</typeparam>
    /// <typeparam name="TValue">The value type produced by the extension.</typeparam>
    public abstract class ExtensionHandler<TData, TValue> : ISyncExtensionHandler<TValue> where TData : class
    {
        /// <summary>The data injected via <see cref="SetData"/>, available to subclasses.</summary>
        protected TData Data { get; private set; }

        /// <summary>Injects the serialized data, cast to <typeparamref name="TData"/>.</summary>
        /// <param name="data">The extension data, or <c>null</c> when unbound.</param>
        public void SetData(object data) => Data = data as TData;

        /// <summary>Produces the extension's value.</summary>
        /// <returns>The current value.</returns>
        public abstract TValue GetValue();
    }
}