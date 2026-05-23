namespace HisaCat.HUE.DataBindEx.Setters
{
    using HisaCat.HUE.Localization;
    using HisaCat.HUE.UnityExtensions;
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
        [SerializeField] private bool m_ResolveJosaTokensForKorean = true;
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

                if (LocalizationManager.SelectedLanguage == SystemLanguage.Korean)
                {
                    if (this.m_ResolveJosaTokensForKorean)
                        value = KoreanUtility.JosaHelper.ResolveJosaTokens(value);
                }
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
        }
#endif
    }
}
