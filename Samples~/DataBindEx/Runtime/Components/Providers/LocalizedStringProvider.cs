namespace HisaCat.Localization.DataBind
{
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    [AddComponentMenu("HisaCat/Data Bind Extension/Providers/[DB] Localized String Provider (Unity)")]
    public class LocalizedStringProvider : DataProvider
    {
        [DataTypeHintExplicit(typeof(string))]
        public DataBinding Key;

        public override object Value
        {
            get
            {
                var key = this.Key.GetValue<string>();
                return LocalizationManager.Load(key);
            }
        }

        public override void Init()
        {
            this.AddBinding(this.Key);
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }
        public override void Deinit()
        {
            this.RemoveBinding(this.Key);
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        protected override void UpdateValue()
        {
            this.OnValueChanged();
        }
        private void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            this.OnValueChanged();
        }
    }
}
