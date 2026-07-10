namespace SST.Configurators
{
    /// <summary>
    /// Marker interface for an extension - a value carrier queried by consumers. Serves as the list element type;
    /// consumers dispatch on whether an entry is an <see cref="IAsyncExtension{TValue}"/> or a sync
    /// <see cref="Extension{T}"/>.
    /// </summary>
    public interface IExtension { }
}