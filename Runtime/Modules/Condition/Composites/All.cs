using System;
using System.Collections.Generic;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Composite condition that is met when every child condition is met (logical AND). An empty set is met.
    /// </summary>
    [Serializable]
    [StableTypeId("SST.Configurators.All")]
    [StableRefCategory("Composite")]
    public class All : CompositeCondition
    {
        /// <summary>The child conditions combined with logical AND.</summary>
        public StableRefList<ICondition> Conditions;

        /// <summary>Returns <c>true</c> if all child conditions are met (or there are none).</summary>
        /// <returns><c>true</c> when every child is satisfied.</returns>
        public override bool IsMet()
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (!(stableRef?.Value?.IsMet() ?? true))
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
    /// Context-aware composite condition that is met when every child condition is met (logical AND). An empty set
    /// is met.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    [Serializable]
    [StableTypeId("SST.Configurators.AllContext")]
    [StableRefCategory("Composite")]
    public class All<TContext> : CompositeCondition<TContext>
    {
        /// <summary>The child conditions combined with logical AND.</summary>
        public StableRefList<ICondition<TContext>> Conditions;

        /// <summary>Returns <c>true</c> if all child conditions are met (or there are none).</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> when every child is satisfied.</returns>
        public override bool IsMet(TContext context)
        {
            if (Conditions == null || Conditions.Count == 0)
                return true;

            foreach (var stableRef in Conditions)
                if (!(stableRef?.Value?.IsMet(context) ?? true))
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