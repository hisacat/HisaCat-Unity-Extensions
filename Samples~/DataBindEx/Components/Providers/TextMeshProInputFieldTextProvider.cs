namespace HisaCat.DataBindEx.Providers
{
    using Slash.Unity.DataBind.Foundation.Providers.Getters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Providers/[DB] TextMeshPro Input Field Text Provider (Unity)")]
    public class TextMeshProInputFieldTextProvider : ComponentDataProvider<TMPro.TMP_InputField, string>
    {
        /// <summary>
        ///   If set, the ValueChanged event is only dispatched when editing of the input field ended (i.e. input field left).
        /// </summary>
        [Tooltip("If set, the ValueChanged event is only dispatched when editing of the input field ended (i.e. input field left).")]
        public bool OnlyUpdateValueOnEndEdit;

        /// <inheritdoc />
        protected override void AddListener(TMPro.TMP_InputField target)
        {
            target.onValueChanged.AddListener(this.OnInputFieldValueChanged);
            target.onEndEdit.AddListener(this.OnInputFieldEndEdit);
        }

        /// <inheritdoc />
        protected override string GetValue(TMPro.TMP_InputField target)
        {
            return target.text;
        }

        /// <inheritdoc />
        protected override void RemoveListener(TMPro.TMP_InputField target)
        {
            target.onValueChanged.RemoveListener(this.OnInputFieldValueChanged);
            target.onEndEdit.RemoveListener(this.OnInputFieldEndEdit);
        }

        private void OnInputFieldEndEdit(string newValue)
        {
            if (this.OnlyUpdateValueOnEndEdit)
            {
                this.OnTargetValueChanged();
            }
        }

        private void OnInputFieldValueChanged(string newValue)
        {
            if (!this.OnlyUpdateValueOnEndEdit)
            {
                this.OnTargetValueChanged();
            }
        }
    }
}
