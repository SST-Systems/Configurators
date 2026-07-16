using System;
using System.Collections.Generic;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Composite condition that is met when no child condition is met (logical NOR). An empty set is met.
    /// </summary>
    [Serializable]
    [StableTypeId("SST.Configurators.None")]
    [StableRefCategory("Composite")]
    public class None : CompositeCondition
    {
        /// <summary>The child conditions; the composite is met only while none of them are.</summary>
        public StableRefList<ICondition> Conditions;

        /// <summary>Returns <c>true</c> if no child condition is met (or there are none).</summary>
        /// <returns><c>true</c> when every child is unsatisfied.</returns>
        public override bool IsMet()
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (stableRef?.Value?.IsMet() ?? false)
                    return false;

            return true;
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

    /// <summary>
    /// Context-aware composite condition that is met when no child condition is met (logical NOR). An empty set is
    /// met.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    [Serializable]
    [StableTypeId("SST.Configurators.NoneContext")]
    [StableRefCategory("Composite")]
    public class None<TContext> : CompositeCondition<TContext>
    {
        /// <summary>The child conditions; the composite is met only while none of them are.</summary>
        public StableRefList<ICondition<TContext>> Conditions;

        /// <summary>Returns <c>true</c> if no child condition is met (or there are none).</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> when every child is unsatisfied.</returns>
        public override bool IsMet(TContext context)
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (stableRef?.Value?.IsMet(context) ?? false)
                    return false;

            return true;
        }

        /// <inheritdoc/>
        public override IEnumerable<ICondition<TContext>> GetConditions()
        {
            if (Conditions == null)
                yield break;

            foreach (var stableRef in Conditions)
                yield return stableRef?.Value;
        }
    }
}