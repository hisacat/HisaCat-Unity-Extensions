#if HUE_UNITY_ADDRESSABLES
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
namespace HisaCat.Addressables
{
    public class AddressablesEditorExtensions
    {
        [MenuItem("HisaCat/Addressable/Sync Addressable Name to Asset Paths")]
        public static void SyncAll()
        {
            if (EditorUtility.DisplayDialog("Addressables", "Sync Addressable Name to Asset Paths?", "Yes", "No") == false) return;

            int changedCount = 0;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var assets = new List<AddressableAssetEntry>();
            settings.GetAllAssets(assets, false);

            foreach (var entry in assets)
            {
                var prevAddress = entry.address;
                var assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (entry.address != assetPath)
                {
                    entry.address = assetPath;
                    changedCount++;
                    Debug.Log($"Addressables: Synced {prevAddress} to {assetPath}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"Addressables: All addresses synced to current asset paths. (Changed: {changedCount})");

            EditorUtility.DisplayDialog("Addressables", $"All addresses synced to current asset paths. (Changed: {changedCount})", "OK");
        }
    }
}
#endif
