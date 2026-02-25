namespace HisaCat.HUE.DataBindEx.Setters
{
    using HisaCat.HUE.Localization;
    using Slash.Unity.DataBind.Core.Presentation;
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] TextMeshPro Localized Text Setter (Unity)")]
    public class TextMeshProLocalizedTextSetter : ComponentSingleSetter<TMPro.TMP_Text, string>, IEditorLocalizedTextUpdateable
    {
        protected override void Reset()
        {
            base.Reset();
            if (this.Data == null) this.Data = new DataBinding();
            this.Data.Type = DataBindingType.Constant;
        }

        protected override void UpdateTargetValue(TMPro.TMP_Text target, string value)
        {
            value = LocalizationManager.Load(value);
            target.SetText(value == null ? string.Empty : value);
        }

        public override void Init()
        {
            base.Init();
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }
        public override void Deinit()
        {
            base.Deinit();
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        private void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            this.OnObjectValueChanged();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateTextOnEditor();
        }

        public void UpdateTextOnEditor()
        {
            if (Application.isPlaying) return;
            if (LocalizationSettings.AutoUpdateLocalizedTextOnEditor == false) return;

            if (this.Data.Type != DataBindingType.Constant) return;
            if (this.TargetBinding.Type != DataBindingType.Reference) return;
            if (this.TargetBinding.Reference is not TMPro.TMP_Text tmp_text) return;

            var text = LocalizationManager.Load(this.Data.Constant);
            if (tmp_text.text == text) return;

            tmp_text.text = text;
            UnityEditor.EditorUtility.SetDirty(tmp_text);
#endif
        }
    }
}
