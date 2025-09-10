using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.Localization
{
    public static class SystemLanguageToLocaleString
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload)) { }
        }
#pragma warning restore IDE0051
#endif

        private static Dictionary<SystemLanguage, string> LocaleMap = new()
        {
            { SystemLanguage.Afrikaans, "af_ZA" },
            { SystemLanguage.Arabic, "ar_SA" },
            { SystemLanguage.Basque, "eu_ES" },
            { SystemLanguage.Belarusian, "be_BY" },
            { SystemLanguage.Bulgarian, "bg_BG" },
            { SystemLanguage.Catalan, "ca_ES" },
            { SystemLanguage.Chinese, "zh_CN" }, // Normally it means Simplified.
            { SystemLanguage.Czech, "cs_CZ" },
            { SystemLanguage.Danish, "da_DK" },
            { SystemLanguage.Dutch, "nl_NL" },
            { SystemLanguage.English, "en_US" },
            { SystemLanguage.Estonian, "et_EE" },
            { SystemLanguage.Faroese, "fo_FO" },
            { SystemLanguage.Finnish, "fi_FI" },
            { SystemLanguage.French, "fr_FR" },
            { SystemLanguage.German, "de_DE" },
            { SystemLanguage.Greek, "el_GR" },
            { SystemLanguage.Hebrew, "he_IL" },
            { SystemLanguage.Hungarian, "hu_HU" },
            { SystemLanguage.Icelandic, "is_IS" },
            { SystemLanguage.Indonesian, "id_ID" },
            { SystemLanguage.Italian, "it_IT" },
            { SystemLanguage.Japanese, "ja_JP" },
            { SystemLanguage.Korean, "ko_KR" },
            { SystemLanguage.Latvian, "lv_LV" },
            { SystemLanguage.Lithuanian, "lt_LT" },
            { SystemLanguage.Norwegian, "no_NO" },
            { SystemLanguage.Polish, "pl_PL" },
            { SystemLanguage.Portuguese, "pt_PT" },
            { SystemLanguage.Romanian, "ro_RO" },
            { SystemLanguage.Russian, "ru_RU" },
            { SystemLanguage.SerboCroatian, "sh_HR" },
            { SystemLanguage.Slovak, "sk_SK" },
            { SystemLanguage.Slovenian, "sl_SI" },
            { SystemLanguage.Spanish, "es_ES" },
            { SystemLanguage.Swedish, "sv_SE" },
            { SystemLanguage.Thai, "th_TH" },
            { SystemLanguage.Turkish, "tr_TR" },
            { SystemLanguage.Ukrainian, "uk_UA" },
            { SystemLanguage.Vietnamese, "vi_VN" },
            { SystemLanguage.ChineseSimplified, "zh_CN" },
            { SystemLanguage.ChineseTraditional, "zh_TW" },
            { SystemLanguage.Hindi, "hi_IN" },
        };

        public static string ToLocaleString(this SystemLanguage language)
        {
            if (LocaleMap.TryGetValue(language, out var localeString) == false)
            {
                Debug.LogError($"[{nameof(ToLocaleString)}] \"{language}\" is unknown language!");
                return null;
            }

            return localeString;
        }
    }
}
