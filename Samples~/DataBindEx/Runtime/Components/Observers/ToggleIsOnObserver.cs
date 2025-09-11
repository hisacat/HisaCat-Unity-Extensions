namespace HisaCat.HUE.DataBindEx.Observers
{
    using Slash.Unity.DataBind.Foundation.Observers;
    using UnityEngine.UI;

    /// <summary>
    ///   Observer for the value of a <see cref="Toggle"/>.
    /// </summary>
    public class ToggleIsOnObserver : ComponentDataObserver<Toggle, bool>
    {
        /// <inheritdoc />
        protected override void AddListener(Toggle target)
        {
            target.onValueChanged.AddListener(this.OnToggleIsOnValueChanged);
        }

        /// <inheritdoc />
        protected override bool GetValue(Toggle target)
        {
            return target.isOn;
        }

        /// <inheritdoc />
        protected override void RemoveListener(Toggle target)
        {
            target.onValueChanged.RemoveListener(this.OnToggleIsOnValueChanged);
        }

        private void OnToggleIsOnValueChanged(bool newValue)
        {
            this.OnTargetValueChanged();
        }
    }
}
