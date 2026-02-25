using UnityEngine;
using UnityEditor;
using HisaCat.IO;
using System.Linq;
using HisaCat.HUE.UnityExtensions;
using System;

namespace HisaCat.HUE.Localization
{
    [InitializeOnLoad]
    public class LocalizationAssetsPostprocessor : AssetPostprocessor
    {
        static LocalizationAssetsPostprocessor()
        {
            LastDefaultLanguage = LocalizationSettings.DefaultLanguage;
        }

        private static readonly string LastDefaultLanguageKey = $"{nameof(LocalizationAssetsPostprocessor)}.{nameof(LastDefaultLanguage)}";
        private static SystemLanguage LastDefaultLanguage
        {
            get => (SystemLanguage)EditorPrefs.GetInt(LastDefaultLanguageKey, (int)SystemLanguage.Unknown);
            set => EditorPrefs.SetInt(LastDefaultLanguageKey, (int)value);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var modifiedAssets = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths);

            static bool isLocalizedJsonAsset(string assetPath)
                => assetPath.EndsWith(UniPath.Combine("Resources", LocalizationSettings.LocalizedJsonsPath, $"{LocalizationSettings.DefaultLanguage.ToLocaleString()}.json"),
                    StringComparison.OrdinalIgnoreCase);

            static bool isLocalizedSettingsAsset(string assetPath)
                => assetPath.EndsWith(UniPath.Combine("Resources", $"{LocalizationSettingsAsset.DefaultAssetPath}.asset"),
                    StringComparison.OrdinalIgnoreCase);

            // If localized json assets was modified.
            if (modifiedAssets.Any(isLocalizedJsonAsset))
            {
                // Clear loaded localized texts.
                LocalizationManager.ClearLoadedLocalizedTexts();

                // Update localized texts.
                LocalizationEditorUtility.UpdateIEditorLocalizedTexts();
                return;
            }

            // If localized settings asset was moved or deleted.
            if (movedFromAssetPaths.Any(isLocalizedSettingsAsset) || deletedAssets.Any(isLocalizedSettingsAsset))
            {
                // Reset last default language.
                LastDefaultLanguage = SystemLanguage.Unknown;
            }

            // If localized settings asset was modified.
            if (modifiedAssets.Any(isLocalizedSettingsAsset))
            {
                // Reload settings asset.
                LocalizationSettings.ReloadSettingsAsset();

                // Clear loaded localized texts.
                LocalizationManager.ClearLoadedLocalizedTexts();

                // If default language was changed.
                if (LastDefaultLanguage != LocalizationSettings.DefaultLanguage)
                {
                    Debug.Log($"[{nameof(LocalizationAssetsPostprocessor)}] Default language changed from \"{LastDefaultLanguage}\" to \"{LocalizationSettings.DefaultLanguage}\".");
                    LastDefaultLanguage = LocalizationSettings.DefaultLanguage;

                    // Generate localized key script.
                    LocalizedKeyScriptGenerator.GenerateLocalizedKeyScript();
                }
            }

            // Generate localized key script if required.
            if (modifiedAssets.ContainsAny(StringComparison.OrdinalIgnoreCase, LocalizedKeyScriptGenerator.SelfScriptPath, LocalizedKeyScriptGenerator.SourceJsonPath))
            {
                // Generate localized key script.
                LocalizedKeyScriptGenerator.GenerateLocalizedKeyScript();
            }
        }
    }
}
