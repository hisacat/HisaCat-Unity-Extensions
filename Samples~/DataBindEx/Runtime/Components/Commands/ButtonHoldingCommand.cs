namespace HisaCat.HUE.DataBindEx.Commands
{
    using HisaCat.HUE.UI;
    using Slash.Unity.DataBind.UI.Unity.Commands;
    using UnityEngine;
    using UnityEngine.Events;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Commands/[DB] Button Holding Command (Unity)")]
    public class ButtonHoldingCommand : UnityEventCommand<ButtonHoldingExtension, float>
    {
        protected override UnityEvent<float> GetEvent(ButtonHoldingExtension target)
        {
            return target.onHolding;
        }
    }
}
