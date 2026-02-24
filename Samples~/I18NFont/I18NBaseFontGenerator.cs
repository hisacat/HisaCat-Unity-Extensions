using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;
using HisaCat.IO;

namespace HisaCat.HUE.Fonts
{
    public static class I18NBaseFontGenerator
    {
        private const string EmptyFontAssetGUID = "f760292733c98584fa662e32677076b2";
        public const string MenuItemName = "Assets/Create/HisaCat/HUE/I18N Fonts/I18N Base Font (Empty Font)";
        [MenuItem(MenuItemName, priority = 0)]
        public static bool GenerateBaseFont()
        {
            var emptyFontAssetPath = AssetDatabase.GUIDToAssetPath(EmptyFontAssetGUID);
            if (string.IsNullOrEmpty(emptyFontAssetPath))
            {
                Debug.LogError($"Empty font asset not found! (GUID: {EmptyFontAssetGUID})");
                return false;
            }
            var assetExtension = Path.GetExtension(emptyFontAssetPath);

            if (Selection.assetGUIDs.Length <= 0)
            {
                Debug.LogError("No asset selected!");
                return false;
            }
            var selectedAssetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (string.IsNullOrEmpty(selectedAssetPath))
            {
                Debug.LogError("Selected asset not found!");
                return false;
            }
            string selectedFolderPath = null;
            if (AssetDatabase.IsValidFolder(selectedAssetPath))
            {
                selectedFolderPath = selectedAssetPath;
            }
            else
            {
                var path = UniPath.NormalizePath(Path.GetDirectoryName(selectedAssetPath));
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Selected asset is not a folder and parent folder not found!");
                    return false;
                }
                selectedFolderPath = path;
            }

            const string fileName = "I18N Base Font";
            var createAssetPath = UniPath.Combine(selectedFolderPath, $"{fileName}{assetExtension}");
            {
                int index = 0;
                while (AssetDatabase.AssetPathExists(createAssetPath))
                    createAssetPath = UniPath.Combine(selectedFolderPath, $"{fileName} ({++index}){assetExtension}");
            }

            AssetDatabase.CopyAsset(emptyFontAssetPath, createAssetPath);
            return true;
        }
    }
}
