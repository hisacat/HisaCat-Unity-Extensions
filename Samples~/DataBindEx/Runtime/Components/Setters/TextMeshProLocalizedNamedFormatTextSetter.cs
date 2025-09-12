namespace HisaCat.HUE.DataBindEx.Setters
{
    using HisaCat.Localization;
    using HisaCat.UnityExtensions;
    using Slash.Unity.DataBind.Core.Presentation;
    using Slash.Unity.DataBind.Foundation.Setters;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/UnityUI/Setters/[DB] TextMeshPro Localized Named Format Text Setter (Unity)")]
    public class TextMeshProLocalizedNamedFormatTextSetter : ComponentSingleSetter<TMPro.TMP_Text, string>, IEditorLocalizedTextUpdateable
    {
        protected override void Reset()
        {
            base.Reset();
            if (this.Data == null) this.Data = new DataBinding();
            this.Data.Type = DataBindingType.Constant;
        }

        [System.Serializable]
        public class NamedFormatArgument
        {
            public string Name = null;
            public DataBinding Argument = null;
        }

        public NamedFormatArgument[] Arguments;
        protected override void UpdateTargetValue(TMPro.TMP_Text target, string value)
        {
            value = LocalizationManager.Load(value);
            int count = Arguments.Length;
            if (count > 0)
            {
                var namedFormatArguments = new StringExtensions.NamedFormatArgument[count];
                for (int i = 0; i < count; i++)
                {
                    var argument = this.Arguments[i];
                    namedFormatArguments[i] = new(argument.Name, argument.Argument.Value);
                }
                value = value.NamedFormat(namedFormatArguments);
            }
            target.SetText(value ?? string.Empty);
        }

        public override void Init()
        {
            base.Init();
            this.Arguments.ForEachFromEndFast(AddBinding);
            void AddBinding(NamedFormatArgument argument) => this.AddBinding(argument.Argument);
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }
        public override void Deinit()
        {
            base.Deinit();
            this.Arguments.ForEachFromEndFast(RemoveBinding);
            void RemoveBinding(NamedFormatArgument argument) => this.RemoveBinding(argument.Argument);
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
                            UnityEditor.Undo.RecordObject(tmp_text, $"[{nameof(TextMeshProLocalizedNamedFormatTextSetter)}] Update text on Editor");
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
