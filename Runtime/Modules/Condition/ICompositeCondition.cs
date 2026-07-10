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
}