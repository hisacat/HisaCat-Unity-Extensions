namespace HisaCat.HUE.DataBindEx.Commands
{
    using HisaCat.HUE.DataBindEx.Observers;
    using Slash.Unity.DataBind.Foundation.Observers;
    using Slash.Unity.DataBind.Foundation.Synchronizers;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///   Synchronizer for the value of a <see cref="Toggle"/>.
    /// </summary>
    [AddComponentMenu("HisaCat/Data Bind Extension/Synchronizers/[DB] Toggle IsOn Synchronizer (Unity)")]
    public class ToggleIsOnSynchronizer : ComponentDataSynchronizer<Toggle, bool>
    {
        private ComponentDataObserver<Toggle, bool> observer;

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            if (this.observer != null)
            {
                this.observer.ValueChanged -= this.OnObserverValueChanged;
                this.observer = null;
            }
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            var target = this.Target;
            if (target != null)
            {
                this.observer = new ToggleIsOnObserver { Target = target };
                this.observer.ValueChanged += this.OnObserverValueChanged;
            }
        }

        /// <inheritdoc />
        protected override void SetTargetValue(Toggle target, bool newContextValue)
        {
            target.isOn = newContextValue;
        }

        private void OnObserverValueChanged()
        {
            this.OnComponentValueChanged(this.Target.isOn);
        }
    }
}
