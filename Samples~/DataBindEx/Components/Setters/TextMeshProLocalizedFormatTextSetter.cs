namespace HisaCat.Localization.DataBind.Setters
{
    using HisaCat.UnityExtensions;
    using Slash.Unity.DataBind.Core.Presentation;
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] TextMeshPro Localized Format Text Setter (Unity)")]
    public class TextMeshProLocalizedFormatTextSetter : ComponentSingleSetter<TMPro.TMP_Text, string>, IEditorLocalizedTextUpdateable
    {
        protected override void Reset()
        {
            base.Reset();
            if (this.Data == null) this.Data = new DataBinding();
            this.Data.Type = DataBindingType.Constant;
        }

        public DataBinding[] Arguments;
        protected override void UpdateTargetValue(TMPro.TMP_Text target, string value)
        {
            value = LocalizationManager.Load(value);
            int count = Arguments.Length;
            if (count > 0)
            {
                var formatArgs = new string[count];
                for (int i = 0; i < count; i++)
                {
                    var formatArg = this.Arguments[i].Value;
                    if (formatArg == null) continue;
                    formatArgs[i] = formatArg.ToString();
                }
                value = string.Format(value, formatArgs);
            }
            target.SetText(value ?? string.Empty);
        }

        public override void Init()
        {
            base.Init();
            this.AddBindings(this.Arguments);
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }
        public override void Deinit()
        {
            base.Deinit();
            this.Arguments.ForEachFromEndFast(this.RemoveBinding);
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        protected override void OnBindingValuesChanged()
        {
            this.OnObjectValueChanged();
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
                            UnityEditor.Undo.RecordObject(tmp_text, $"[{nameof(TextMeshProLocalizedFormatTextSetter)}] Update text on Editor");
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
