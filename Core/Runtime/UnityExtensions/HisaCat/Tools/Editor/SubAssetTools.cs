using UnityEditor;
using UnityEngine;

namespace HisaCat.Tools
{
    public static class SubAssetTools
    {
        [MenuItem("HisaCat/Sub Asset Tools/Rename")]
        public static void OpenSubAssetRenameWindow()
        {
            SubAssetRenameWindow.ShowWindow();
        }

        public class SubAssetRenameWindow : EditorWindow
        {
            private Object targetAsset;
            private string newName = string.Empty;
            private string errorMessage = string.Empty;
            private string successMessage = string.Empty;

            public static void ShowWindow()
            {
                var window = GetWindow<SubAssetRenameWindow>(true, "Nested Asset Rename Window");
                window.minSize = new Vector2(400, 200);
                window.Show();
            }

            private void OnGUI()
            {
                GUILayout.Label("Nested Asset Rename Tool", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();
                targetAsset = EditorGUILayout.ObjectField("Target Asset", targetAsset, typeof(Object), false);
                if (EditorGUI.EndChangeCheck())
                {
                    successMessage = null;
                    if (!IsValidNestedAsset(targetAsset))
                    {
                        targetAsset = null;
                        errorMessage = "Target object is not in project or nested asset.";
                    }
                    else
                    {
                        errorMessage = string.Empty;
                        newName = targetAsset.name;
                    }
                }

                EditorGUI.BeginChangeCheck();
                newName = EditorGUILayout.TextField("New Name", newName);
                if (EditorGUI.EndChangeCheck())
                {
                    successMessage = null;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);

                EditorGUI.BeginDisabledGroup(targetAsset == null || newName == targetAsset.name);
                if (GUILayout.Button("Rename"))
                {
                    if (RenameAsset(targetAsset, newName))
                        successMessage = "Rename success!";
                }
                EditorGUI.EndDisabledGroup();

                if (!string.IsNullOrEmpty(successMessage))
                    EditorGUILayout.HelpBox(successMessage, MessageType.Info);
            }

            private bool IsValidNestedAsset(Object obj)
            {
                if (obj == null) return false;
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) return false;

                return AssetDatabase.IsSubAsset(obj);
            }

            private bool RenameAsset(Object obj, string newName)
            {
                if (obj == null || string.IsNullOrEmpty(newName))
                    return false;

                obj.name = newName;
                EditorUtility.SetDirty(obj);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return true;
            }
        }
    }
}
