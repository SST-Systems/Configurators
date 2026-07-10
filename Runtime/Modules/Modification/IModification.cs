namespace SST.Configurators
{
    /// <summary>
    /// Marker interface for a modification - an operation applied to a <typeparamref name="TContext"/>. Serves as
    /// the list element type; processors dispatch on whether an entry is an <see cref="IAsyncModification{TContext}"/>
    /// or a sync <see cref="Modification{TContext}"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type the modification is applied to.</typeparam>
    public interface IModification<in TContext> { }
}
