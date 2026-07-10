using System;
using System.Collections.Generic;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Composite condition that is met when at least one child condition is met (logical OR). An empty set is not met.
    /// </summary>
    [Serializable]
    [StableRefCategory("Composite")]
    public class Any : CompositeCondition
    {
        /// <summary>The child conditions combined with logical OR.</summary>
        public StableRefList<ICondition> Conditions;

        /// <summary>Returns <c>true</c> if at least one child condition is met.</summary>
        /// <returns><c>true</c> when any child is satisfied.</returns>
        public override bool IsMet()
        {
            if (Conditions == null || Conditions.Count == 0)
                return false;

            foreach (var stableRef in Conditions)
                if (stableRef?.Value?.IsMet() ?? false)
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public override IEnumerable<ICondition> GetConditions()
        {
            if (Conditions == null)
                yield break;
            
            foreach (var stableRef in Conditions)
                yield return stableRef?.Value;
        }
    }
}