namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Ready-made two-state widget (Active / Deactive). Author each state's instructions in the Inspector.
    /// Drive it manually via SetState, or attach a <see cref="DefaultStatefulWidgetController"/> to switch
    /// states from a combination of toggle conditions.
    /// </summary>
    public class DefaultStatefulWidget : StatefulWidget<DefaultState>
    {
    }

    /// <summary>The two states this widget can be in — shown (Active) or hidden (Deactive).</summary>
    public enum DefaultState
    {
        Active,
        Deactive
    }
}