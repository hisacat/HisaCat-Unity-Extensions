using UnityEngine;
using UnityEditor;
using HisaCat.IO;

namespace HisaCat.HUE.Localization
{
    public class LocalizedTextPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (asset.EndsWith(UniPath.Combine("Resources", LocalizationSettings.LocalizedJsonsPath, $"{LocalizationSettings.DefaultLanguage.ToLocaleString()}.json"), System.StringComparison.OrdinalIgnoreCase))
                {
                    // Clear localized text dictionary when locale json was changed.
                    LocalizationManager.ClearLoadedLocalizedTexts();

                    LocalizationEditorUtility.UpdateIEditorLocalizedTexts();
                    return;
                }
            }
        }
    }
}
