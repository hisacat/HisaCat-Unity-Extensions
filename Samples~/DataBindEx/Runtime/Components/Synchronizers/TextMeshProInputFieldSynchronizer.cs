namespace HisaCat.HUE.DataBindEx.Synchronizers
{
    using HisaCat.HUE.DataBindEx.Observers;
    using Slash.Unity.DataBind.Foundation.Synchronizers;
    using UnityEngine;

    /// <summary>
    ///   Synchronizer for the text of a <see cref="TextMeshProInputFieldSynchronizer"/>.
    /// </summary>
    [AddComponentMenu("HisaCat/Data Bind Extension/Synchronizers/[DB] TextMeshPro Input Field Text Synchronizer (Unity)")]
    public class TextMeshProInputFieldSynchronizer : ComponentDataSynchronizer<TMPro.TMP_InputField, string>
    {
        /// <summary>
        ///     If set, the ValueChanged event is only dispatched when editing of the input field ended (i.e. input field left).
        /// </summary>
        [Tooltip(
            "If set, the ValueChanged event is only dispatched when editing of the input field ended (i.e. input field left).")]
        public bool OnlyUpdateValueOnEndEdit;

        private TextMeshProInputFieldTextObserver observer;

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
                this.observer = new TextMeshProInputFieldTextObserver
                {
                    OnlyUpdateValueOnEndEdit = this.OnlyUpdateValueOnEndEdit,
                    Target = target
                };
                this.observer.ValueChanged += this.OnObserverValueChanged;
            }
        }

        /// <inheritdoc />
        protected override void SetTargetValue(TMPro.TMP_InputField target, string newContextValue)
        {
            target.text = newContextValue;
        }

        private void OnObserverValueChanged()
        {
            this.OnComponentValueChanged(this.Target.text);
        }
    }
}
