namespace HisaCat.HUE.DataBindEx.UI.Unity.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] Slider Max Value Setter (Unity)")]
    public class SliderMaxValueSetter : ComponentSingleSetter<Slider, float>
    {
        protected override void UpdateTargetValue(Slider target, float value)
        {
            target.maxValue = value;
        }
    }
}
