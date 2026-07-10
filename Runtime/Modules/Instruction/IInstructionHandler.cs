namespace SST.Configurators
{
    /// <summary>
    /// Marker interface used as the pool key for instruction handlers. Handlers are pooled and not reset on
    /// return, so they must keep no per-call state; reset any state in <c>SetData</c> or <c>Apply</c>.
    /// </summary>
    public interface IInstructionHandler { }

    /// <summary>
    /// A synchronous instruction handler that receives injected data and executes on demand.
    /// </summary>
    public interface ISyncInstructionHandler : IInstructionHandler
    {
        /// <summary>Injects the serialized data into the handler.</summary>
        /// <param name="data">The instruction data, or <c>null</c> when unbound.</param>
        void SetData(object data);

        /// <summary>Executes the instruction.</summary>
        void Apply();
    }
}