using System;
using SST.StableRef;
using UnityEngine.UI;

namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Condition data: the serialized part edited in the inspector. Just holds the <see cref="Toggle"/> to watch;
    /// the paired <see cref="ToggleOnHandler"/> supplies the runtime logic.
    /// </summary>
    [Serializable]
    [StableRefCategory("UI")]
    public class ToggleOn : ConditionData<ToggleOnHandler>
    {
        public Toggle Toggle;
    }

    /// <summary>
    /// Runtime handler for <see cref="ToggleOn"/>. Subscribes to <c>Toggle.onValueChanged</c> only while someone
    /// is listening, reports the toggle's state via <see cref="IsMet"/>, and calls <c>NotifyChanged</c> on each
    /// change so listeners re-evaluate — no per-frame polling.
    /// </summary>
    public class ToggleOnHandler : ConditionHandler<ToggleOn>
    {
        protected override void OnFirstListenerAdded()
        {
            if (Data.Toggle)
                Data.Toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        protected override void OnLastListenerRemoved()
        {
            if (Data.Toggle)
                Data.Toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        public override bool IsMet() => Data.Toggle && Data.Toggle.isOn;

        private void OnToggleChanged(bool value) => NotifyChanged();
    }
}