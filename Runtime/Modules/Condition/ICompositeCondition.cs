using System.Collections.Generic;

namespace SST.Configurators
{
    /// <summary>
    /// A condition composed of other conditions. Enables the manager to recursively bind, release, and subscribe
    /// to nested conditions. See <see cref="All"/>, <see cref="Any"/>, <see cref="None"/>, and <see cref="Not"/>.
    /// </summary>
    public interface ICompositeCondition : ICondition
    {
        /// <summary>Returns the child conditions this composite combines.</summary>
        /// <returns>The nested conditions; entries may be <c>null</c>.</returns>
        IEnumerable<ICondition> GetConditions();
    }

    /// <summary>
    /// A context-aware condition composed of other conditions over the same <typeparamref name="TContext"/>.
    /// Enables the manager to recursively bind, release, and subscribe to nested conditions. See
    /// <see cref="All{TContext}"/>, <see cref="Any{TContext}"/>, <see cref="None{TContext}"/>, and
    /// <see cref="Not{TContext}"/>. For the context-free variant see <see cref="ICompositeCondition"/>.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    public interface ICompositeCondition<TContext> : ICondition<TContext>
    {
        /// <summary>Returns the child conditions this composite combines.</summary>
        /// <returns>The nested conditions; entries may be <c>null</c>.</returns>
        IEnumerable<ICondition<TContext>> GetConditions();
    }
}