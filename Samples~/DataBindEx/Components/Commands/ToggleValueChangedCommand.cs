namespace HisaCat.DataBindEx.Commands
{
    using Slash.Unity.DataBind.UI.Unity.Commands;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [AddComponentMenu("HisaCat/Data Bind Extension/Commands/[DB] Toggle Value Changed Command (Unity)")]
    public class ToggleValueChangedCommand : UnityEventCommand<Toggle, bool>
    {
        protected override UnityEvent<bool> GetEvent(Toggle target)
        {
            return target.onValueChanged;
        }
    }
}
