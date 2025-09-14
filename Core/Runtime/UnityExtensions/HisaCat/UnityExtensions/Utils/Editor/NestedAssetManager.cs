#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HisaCat.Editors
{
    public class NestedAssetManager : EditorWindow
    {
        [MenuItem("HisaCat/NestedAsset/Open NestedAssetManager Window")]
        private static void Init()
        {
            NestedAssetManager window = (NestedAssetManager)EditorWindow.GetWindow(typeof(NestedAssetManager));
            window.titleContent = new GUIContent("Nested Asset Manager");
            window.Show();
        }

        private Object containerObject = null;
        private Object targetObject = null;
        private bool removeTargetAssetAfterAdd = false;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Nested Asset Manager", EditorStyles.largeLabel);
            EditorGUILayout.Space();

            this.containerObject = EditorGUILayout.ObjectField("Container", this.containerObject, typeof(Object), allowSceneObjects: false);
            this.targetObject = EditorGUILayout.ObjectField("Target", this.targetObject, typeof(Object), allowSceneObjects: false);

            this.removeTargetAssetAfterAdd = EditorGUILayout.Toggle("Remove after add?", this.removeTargetAssetAfterAdd);
            GUI.enabled = this.containerObject != null && this.targetObject != null;
            {
                if (GUILayout.Button("Add"))
                {
                    if (AssetDatabase.IsSubAsset(this.targetObject))
                    {
                        EditorUtility.DisplayDialog("Nested Asset Manager", "Target asset is already subAsset!", "Ok");
                        return;
                    }

                    Object newInstance = null;
                    if (this.targetObject is GameObject)
                        EditorUtility.DisplayDialog("Nested Asset Manager", "GameObject Cannot be added!", "Ok");
                    else
                    {
                        newInstance = Object.Instantiate(this.targetObject);
                        newInstance.name = this.targetObject.name;
                    }

                    if (newInstance != null)
                    {
                        AssetDatabase.AddObjectToAsset(newInstance, this.containerObject);
                        if (this.removeTargetAssetAfterAdd)
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this.targetObject));

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                if (GUILayout.Button("Detach"))
                {
                    if (AssetDatabase.IsSubAsset(this.targetObject) == false)
                        EditorUtility.DisplayDialog("Nested Asset Manager", "Target asset is not subAsset!", "Ok");
                    else
                    {
                        var containerPath = AssetDatabase.GetAssetPath(this.containerObject);
                        var targetPath = AssetDatabase.GetAssetPath(this.targetObject);
                        if (containerPath != targetPath)
                        {
                            EditorUtility.DisplayDialog("Nested Asset Manager", "Target is not subAsset of container!", "Ok");
                        }
                        else
                        {
                            var assetPath = containerPath.Substring(0, containerPath.Length - System.IO.Path.GetFileName(containerPath).Length);
                            var extension = ".asset";
                            if (this.targetObject is AnimatorOverrideController)
                                extension = ".overrideController";
                            else if (this.targetObject is AnimationClip)
                                extension = ".anim";
                            else
                            {
                                EditorUtility.DisplayDialog("Nested Asset Manager", "Unknown type! please write file extension at this script", "Ok");
                                extension = null;
                            }

                            if (string.IsNullOrEmpty(extension) == false)
                            {
                                assetPath += this.targetObject.name + extension;
                                var newInstance = Object.Instantiate(this.targetObject);
                                AssetDatabase.CreateAsset(newInstance, assetPath);
                                AssetDatabase.RemoveObjectFromAsset(this.targetObject);

                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }
                    }
                }
            }
            GUI.enabled = true;
        }

        private void SetIndent(System.Action draw) => SetIndent(1, draw);
        private void SetIndent(int amount, System.Action draw)
        {
            EditorGUI.indentLevel += amount;
            draw.Invoke();
            EditorGUI.indentLevel -= amount;
        }
    }
}
#endif
