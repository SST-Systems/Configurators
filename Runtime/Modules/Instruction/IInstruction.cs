namespace SST.Configurators
{
    /// <summary>
    /// Marker interface for an instruction - a single, context-free command in a sequence. Serves as the list
    /// element type; processors dispatch on whether an entry is an <see cref="IAsyncInstruction"/> or a sync
    /// <see cref="Instruction"/>.
    /// </summary>
    public interface IInstruction { }
}