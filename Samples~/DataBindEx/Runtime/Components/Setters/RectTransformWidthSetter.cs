namespace HisaCat.HUE.DataBindEx.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] RectTransform Width Setter (Unity)")]
    public class RectTransformWidthSetter : ComponentSingleSetter<RectTransform, float>
    {
        protected override void UpdateTargetValue(RectTransform target, float value)
        {
            var sizeDelta = target.sizeDelta;
            sizeDelta.x = value;
            target.sizeDelta = sizeDelta;
        }
    }
}
