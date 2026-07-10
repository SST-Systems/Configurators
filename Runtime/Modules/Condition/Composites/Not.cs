using System;
using System.Collections.Generic;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Composite condition that inverts a single child condition (logical NOT). A missing child is treated as not met.
    /// </summary>
    [Serializable]
    [StableRefCategory("Composite")]
    public class Not : CompositeCondition
    {
        /// <summary>The child condition to negate.</summary>
        public StableRef<ICondition> Condition;

        /// <summary>Returns the logical negation of the child condition.</summary>
        /// <returns><c>true</c> when the child is unsatisfied or missing.</returns>
        public override bool IsMet() => !(Condition?.Value?.IsMet() ?? false);

        /// <inheritdoc/>
        public override IEnumerable<ICondition> GetConditions() { yield return Condition?.Value; }
    }
}