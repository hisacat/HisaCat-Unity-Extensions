using HisaCat.Localization;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    /// <summary>
    /// Updates I18N base font fallbacks when the language changes in the editor.<br/>
    /// This lets you preview multilingual font results in real time during development.
    /// </summary>
    [InitializeOnLoad]
    public class I18NBaseFontUpdateOnEditor
    {
        static I18NBaseFontUpdateOnEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            SubscribeToLanguageChanged();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // LocalizationManager clears OnLanguageChanged when entering play mode (domain reload disabled).
            // Re-subscribe when returning to edit mode so language changes still update fonts.
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SubscribeToLanguageChanged();
            }
        }

        /// <summary>
        /// Subscribe to <see cref="LocalizationManager.OnLanguageChanged"/> event.
        /// </summary>
        private static void SubscribeToLanguageChanged()
        {
            // Unsubscribe first so this is idempotent (safe to call multiple times).
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private static void OnLanguageChanged(SystemLanguage prevLang, SystemLanguage curLang)
        {
            Debug.Log($"[{nameof(I18NBaseFontUpdateOnEditor)}] {nameof(OnLanguageChanged)}: Language changed from {prevLang} to {curLang}");
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
