namespace HisaCat.HUE.DataBindEx.Setters
{
    using HisaCat.Localization;
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

        private void OnValidate()
        {
            UpdateTextOnEditor();
        }

        public void UpdateTextOnEditor()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false && LocalizationSettings.AutoUpdateLocalizedTextOnEditor)
            {
                if (this.Data.Type == DataBindingType.Constant)
                {
                    if (this.TargetBinding.Type != DataBindingType.Reference) return;
                    var tmp_text = this.TargetBinding.Reference as TMPro.TMP_Text;
                    if (tmp_text != null)
                    {
                        var text = LocalizationManager.Load(this.Data.Constant ?? string.Empty);
                        if (tmp_text.text != text)
                        {
                            UnityEditor.Undo.RecordObject(tmp_text, $"[{nameof(TextMeshProLocalizedTextSetter)}] Update text on Editor");
                            tmp_text.text = text;
                            UnityEditor.EditorUtility.SetDirty(tmp_text);
                        }
                    }
                }
            }
#endif
        }
    }
}
