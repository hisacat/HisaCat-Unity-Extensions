namespace HisaCat.HUE.DataBindEx.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] TextMeshPro Text Setter (Unity)")]
    public class TextMeshProTextSetter : ComponentSingleSetter<TMPro.TMP_Text, string>
    {
        protected override void UpdateTargetValue(TMPro.TMP_Text target, string value)
        {
            target.SetText(value ?? string.Empty);
        }
    }
}
