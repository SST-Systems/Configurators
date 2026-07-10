namespace SST.Configurators
{
    /// <summary>
    /// Base class for a synchronous instruction handler, exposing the injected data to subclasses.
    /// Because handlers are pooled and reused, keep no per-call state outside <see cref="Data"/>.
    /// </summary>
    /// <typeparam name="TData">The serialized data type this handler operates on.</typeparam>
    public abstract class InstructionHandler<TData> : ISyncInstructionHandler where TData : class
    {
        /// <summary>The data injected via <see cref="SetData"/>, available to subclasses.</summary>
        protected TData Data { get; private set; }

        /// <summary>Injects the serialized data, cast to <typeparamref name="TData"/>.</summary>
        /// <param name="data">The instruction data, or <c>null</c> when unbound.</param>
        public void SetData(object data) => Data = data as TData;

        /// <summary>Executes the instruction.</summary>
        public abstract void Apply();
    }
}