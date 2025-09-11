namespace HisaCat.HUE.DataBindEx.UI.Unity.Setters
{
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] Animator Float Setter (Unity)")]
    public class AnimatorFloatSetter : AnimatorParameterSetter<float>
    {
        protected override void SetAnimatorParameter(Animator target, float newValue)
        {
            target.SetFloat(this.AnimatorParameterName, newValue);
        }
    }
}
