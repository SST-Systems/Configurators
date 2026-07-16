using System;
using System.Collections.Generic;
using SST.StableRef;

namespace SST.Configurators
{
    /// <summary>
    /// Composite condition that is met when at least one child condition is met (logical OR). An empty set is not met.
    /// </summary>
    [Serializable]
    [StableTypeId("SST.Configurators.Any")]
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

    /// <summary>
    /// Context-aware composite condition that is met when at least one child condition is met (logical OR). An empty
    /// set is not met.
    /// </summary>
    /// <typeparam name="TContext">The context the condition is evaluated against.</typeparam>
    [Serializable]
    [StableTypeId("SST.Configurators.AnyContext")]
    [StableRefCategory("Composite")]
    public class Any<TContext> : CompositeCondition<TContext>
    {
        /// <summary>The child conditions combined with logical OR.</summary>
        public StableRefList<ICondition<TContext>> Conditions;

        /// <summary>Returns <c>true</c> if at least one child condition is met.</summary>
        /// <param name="context">The context to evaluate against.</param>
        /// <returns><c>true</c> when any child is satisfied.</returns>
        public override bool IsMet(TContext context)
        {
            if (Conditions == null || Conditions.Count == 0)
                return false;

            foreach (var stableRef in Conditions)
                if (stableRef?.Value?.IsMet(context) ?? false)
                    return true;

            return false;
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