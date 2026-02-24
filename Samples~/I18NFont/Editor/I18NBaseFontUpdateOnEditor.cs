using HisaCat.Localization;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    /// <summary>
    /// Update I18N Base Font fallbacks when the language changes.<br/>
    /// Its works only in the editor environment.<br/>
    /// This allows you to preview multilingual font application results in real time during development.
    /// </summary>
    [InitializeOnLoad]
    public class I18NBaseFontUpdateOnEditor
    {
        static I18NBaseFontUpdateOnEditor()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }
        static void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            if (EditorApplication.isPlaying == false)
            {
                Debug.Log($"[{nameof(I18NBaseFontUpdateOnEditor)}] {nameof(OnLanguageChanged)}: Update all I18NFonts... (Language: {curLang})");

                var assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(I18NFontDataAsset)}");
                foreach (var assetGUID in assetGUIDs)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    var i18nFontDataAsset = AssetDatabase.LoadAssetAtPath<I18NFontDataAsset>(assetPath);
                    if (i18nFontDataAsset == null) continue;
                    I18NFontUtility.UpdateBaseFontFallbacks(i18nFontDataAsset, i18nFontDataAsset.BaseFont, curLang, forceRefreshAllTmpFonts: true);
                    EditorUtility.SetDirty(i18nFontDataAsset.BaseFont);

                    Debug.Log($"[{nameof(I18NBaseFontUpdateOnEditor)}] {nameof(OnLanguageChanged)}: {i18nFontDataAsset.name} Updated.", i18nFontDataAsset);
                }
            }
        }
    }
}
