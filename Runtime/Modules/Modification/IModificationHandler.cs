namespace SST.Configurators
{
    /// <summary>
    /// Marker interface used as the pool key for modification handlers. Handlers are pooled and not reset on
    /// return, so they must keep no per-call state; context flows in as a parameter.
    /// </summary>
    public interface IModificationHandler { }

    /// <summary>
    /// A synchronous modification handler that receives injected data and applies to a context on demand.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    public interface ISyncModificationHandler<in TContext> : IModificationHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The modification data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Applies the modification to the given context.</summary>
        /// <param name="context">The context to modify.</param>
        void Apply(TContext context);
    }
}