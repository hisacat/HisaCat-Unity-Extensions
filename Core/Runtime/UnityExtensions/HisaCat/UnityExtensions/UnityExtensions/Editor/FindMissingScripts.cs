#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HisaCat.UnityExtensions.Editors
{
    public class FindMissingScripts : EditorWindow
    {
        [MenuItem("AssetHelper/FindMissingScripts")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FindMissingScripts));
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Find Missing Scripts in selected prefabs"))
            {
                FindInSelected();
            }
            if (GUILayout.Button("Find Missing Scripts in project"))
            {
                FindInProject();
            }
        }
        private static void FindInSelected()
        {
            GameObject[] go = Selection.gameObjects;
            int go_count = 0, components_count = 0, missing_count = 0;
            foreach (GameObject g in go)
            {
                go_count++;

                var components = GetComponentsAllRecursive<Component>(g, true);
                for (int i = 0; i < components.Count; i++)
                {
                    components_count++;
                    if (components[i] == null)
                    {
                        missing_count++;
                        string s = g.name;
                        Transform t = g.transform;
                        while (t.parent != null)
                        {
                            s = t.parent.name + "/" + s;
                            t = t.parent;
                        }
                        Debug.Log(s + " has an empty script attached in position: " + i, g);
                    }
                }
            }

            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
        }

        //and need checks gameobjects inner scenes...
        private static void FindInProject()
        {
            var assets = AssetDataBaseHelper.GetAllPrefabs();
            int go_count = 0, components_count = 0, missing_count = 0;
            foreach (var path in assets)
            {
                var g = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (g == null) continue;
                go_count++;

                var components = GetComponentsAllRecursive<Component>(g, true);
                for (int i = 0; i < components.Count; i++)
                {
                    components_count++;
                    if (components[i] == null)
                    {
                        missing_count++;
                        string s = g.name;
                        Transform t = g.transform;
                        while (t.parent != null)
                        {
                            s = t.parent.name + "/" + s;
                            t = t.parent;
                        }
                        Debug.Log(s + " has an empty script attached in position: " + i, g);
                    }
                }
            }

            Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
        }

        private static List<T> GetComponentsAllRecursive<T>(GameObject gameObject, bool includeInactive) where T : Component
        {
            var result = new List<T>();
            GetComponentsAllRecursive<T>(result, gameObject, includeInactive);
            return result;
        }
        private static void GetComponentsAllRecursive<T>(List<T> result, GameObject gameObject, bool includeInactive) where T : Component
        {
            var components = gameObject.GetComponents<T>();
            foreach (var component in components)
                result.Add(component);

            int count = gameObject.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = gameObject.transform.GetChild(i);
                GetComponentsAllRecursive<T>(result, child.gameObject, includeInactive);
            }
        }
    }
}
#endif
