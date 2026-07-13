namespace SST.Configurators.Samples.ConditionsForVisibility
{
    /// <summary>
    /// Drives a <see cref="DefaultStatefulWidget"/> from toggle conditions. Author rules in the Inspector,
    /// ordered by priority, e.g.: Active (When: All of ToggleOn A + ToggleOn B), Deactive (When: empty = always).
    /// The first rule whose toggle combination is met wins.
    /// </summary>
    public class DefaultStatefulWidgetController : StatefulWidgetController<DefaultState>
    {
    }
}