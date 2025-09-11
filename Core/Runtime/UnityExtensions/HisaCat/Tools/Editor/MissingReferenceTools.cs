using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using HisaCat.UnityExtensions;
using System.Linq;
using System.Collections.Generic;

namespace HisaCat.Tools
{
    public static class MissingReferenceFinder
    {
        private const string FindMissingGameObjectsMenuPath = "HisaCat/Missing Reference Tools/Find Missing GameObjects";
        [MenuItem(FindMissingGameObjectsMenuPath, false, 0)]
        public static void FindMissingGameObjects()
        {
            int missingCount = 0;
            var allGameObjects = SceneExtensions.GetAllGameObjectsRecursive();
            foreach (var go in allGameObjects)
            {
                // Prefab 인스턴스인데 원본이 사라진 경우
                var prefabType = PrefabUtility.GetPrefabAssetType(go);
                if (prefabType == PrefabAssetType.MissingAsset)
                {
                    Debug.Log($"Missing GameObject \"{go.name}\" found.\r\nPath: {go.transform.GetFullPath()}", go);
                    missingCount++;
                }
            }

            EditorUtility.DisplayDialog("Missing GameObjects", $"{missingCount} of missing GameObjects found.", "OK");
        }

        private const string FindMissingComponentsMenuPath = "HisaCat/Missing Reference Tools/Find Missing Components";
        [MenuItem(FindMissingComponentsMenuPath, false, 1)]
        public static void FindMissingComponents()
        {
            int missingCount = 0;
            var allGameObjects = SceneExtensions.GetAllGameObjectsRecursive();
            foreach (var go in allGameObjects)
            {
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        Debug.Log($"Missing Component found in GameObject \"{go.name}\".\r\nPath: {go.transform.GetFullPath()}", go);
                        missingCount++;
                    }
                }
            }
            EditorUtility.DisplayDialog("Missing Components", $"{missingCount} of missing Components found.", "OK");
        }

        private const string FindMissingObjectReferencesMenuPath = "HisaCat/Missing Reference Tools/Find Missing Object References";
        [MenuItem(FindMissingObjectReferencesMenuPath, false, 2)]
        public static void FindMissingObjectReferences()
        {
            int missingCount = 0;
            var allGameObjects = SceneExtensions.GetAllGameObjectsRecursive();
            foreach (var go in allGameObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    var so = new SerializedObject(comp);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                            {
                                Debug.Log($"Missing Object Reference found in \"{go.name}\". Component: \"{comp.GetType().Name}\" Property: \"{prop.displayName}\"\r\nPath: {go.transform.GetFullPath()}", go);
                                missingCount++;
                            }
                        }
                    }
                }
            }
            EditorUtility.DisplayDialog("Missing Object References", $"{missingCount} of missing Object References found.", "OK");
        }
    }
}
