#if UNITY_EDITOR
using HisaCat.UnityExtensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HisaCat.Editors
{
    public class LayerChangerWindow : EditorWindow
    {
        private int targetLayerIndex;
        private int newLayerIndex;
        private bool updateSceneObjects;
        private bool updatePrefabs;

        [MenuItem("HisaCat/Utils/Layer Changer")]
        public static void ShowWindow()
        {
            GetWindow<LayerChangerWindow>("Layer Changer");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("This utility is change layers for whole assets in project..\nYou cannot undo. Make sure backup.", MessageType.Info);

            this.targetLayerIndex = EditorGUILayout.IntField("Target Layer Index:", this.targetLayerIndex);
            this.newLayerIndex = EditorGUILayout.IntField("New Layer Index:", this.newLayerIndex);
            this.updateSceneObjects = EditorGUILayout.Toggle("Update Scene Objects", this.updateSceneObjects);
            this.updatePrefabs = EditorGUILayout.Toggle("Update Prefabs", this.updatePrefabs);

            GUI.enabled = this.updateSceneObjects || this.updatePrefabs;
            if (GUILayout.Button("Do Change"))
            {
                var targetLayerName = LayerMask.LayerToName(this.targetLayerIndex);
                var newLayerName = LayerMask.LayerToName(this.newLayerIndex);
                if (EditorUtility.DisplayDialog("Layer Changer", $"Are you sure to change layer {this.targetLayerIndex}({targetLayerName}) to {this.newLayerIndex}({newLayerName})?", "Yes", "No"))
                {
                    ChangeLayers();
                    EditorUtility.DisplayDialog("Layer Changer", "Done.", "Ok");
                }
            }
        }

        private void ChangeLayers()
        {
            if (this.updateSceneObjects)
            {
                // Store the original open scene
                string originalScenePath = EditorSceneManager.GetActiveScene().path;

                // Get all scene paths in the project
                string[] allScenePaths = AssetDatabase.GetAllAssetPaths();
                allScenePaths = System.Array.FindAll(allScenePaths, path =>
                    path.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) &&
                    path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase));

                // Iterate over all scenes and change the layers
                foreach (string scenePath in allScenePaths)
                {
                    int changedCount = 0;
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    var rootObjects = scene.GetRootGameObjects();
                    foreach (GameObject rootObject in rootObjects)
                    {
                        var objects = rootObject.GetNestedAllChidrenList();
                        objects.Add(rootObject);

                        foreach (var obj in objects)
                        {
                            bool doUpdate = false;

                            // If object is instance of prefab. update only layer is modified.
                            if (PrefabUtility.IsPartOfPrefabInstance(obj))
                            {
                                var modifications = PrefabUtility.GetPropertyModifications(obj);
                                if (System.Array.Exists(modifications, e => e.propertyPath == "m_Layer"))
                                    doUpdate = true;
                            }
                            else
                            {
                                doUpdate = true;
                            }

                            if (doUpdate)
                            {
                                if (obj.layer == targetLayerIndex)
                                {
                                    obj.layer = newLayerIndex;
                                    changedCount++;
                                }
                            }
                        }
                    }

                    if (changedCount > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        Debug.Log($"[Layer Changer] Scene updated {changedCount} objects. path: {scene.path}");
                    }

                    if (scene.path != scene.path) EditorSceneManager.CloseScene(scene, true);
                }

                // Open the original scene again
                EditorSceneManager.OpenScene(originalScenePath);
            }

            if (this.updatePrefabs)
            {
                // Get all prefab assets in the project
                string[] allPrefabPaths = AssetDatabase.GetAllAssetPaths();
                allPrefabPaths = System.Array.FindAll(allPrefabPaths, path =>
                    path.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase) &&
                    path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase));

                // Iterate over all prefabs and change the layers
                foreach (string prefabPath in allPrefabPaths)
                {
                    int changedCount = 0;

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var objects = prefab.GetNestedAllChidrenList();
                    objects.Add(prefab);

                    foreach (var obj in objects)
                    {
                        if (obj.layer == targetLayerIndex)
                        {
                            obj.layer = newLayerIndex;
                            changedCount++;
                        }
                    }
                    if (changedCount > 0)
                    {
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"[Layer Changer] Prefab updated {changedCount} objects. path: {prefabPath}");
                    }
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
#endif
