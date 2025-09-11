namespace HisaCat.HUE.DataBindEx.Commands
{
    using Slash.Unity.DataBind.UI.Unity.Commands;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [AddComponentMenu("HisaCat/Data Bind Extension/Commands/[DB] Toggle On Command (Unity)")]
    public class ToggleOnCommand : UnityEventCommand<Toggle>
    {
        private UnityEvent unityEvent = null;
        protected override UnityEvent GetEvent(Toggle target)
        {
            this.unityEvent = new UnityEvent();

            target.onValueChanged.RemoveListener(OnValueChanged);
            target.onValueChanged.AddListener(OnValueChanged);

            return this.unityEvent;
        }

        private void OnValueChanged(bool isOn)
        {
            if (isOn) this.unityEvent.Invoke();
        }
    }
}
