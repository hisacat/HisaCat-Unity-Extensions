using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HisaCat.UnityExtensions.Editors
{
    public static class AssetDataBaseHelper
    {
        public static string[] GetAllPrefabs()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (s.Contains(".prefab")) result.Add(s);
            }
            return result.ToArray();
        }

        public static List<T> GetAllComponentsInProject<T>() where T : Component
        {
            var allPrefabs = GetAllPrefabs();
            var result = new List<T>();

            foreach (string prefab in allPrefabs)
            {
                UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(prefab);
                GameObject go;
                try
                {
                    go = (GameObject)o;
                    T[] components = go.GetComponentsInChildren<T>(true);
                    foreach (T c in components)
                    {
                        result.Add(c);
                    }
                }
                catch
                {
                    Debug.Log("For some reason, prefab " + prefab + " won't cast to GameObject");
                }
            }

            return result;
        }

        public enum AssetReferenceType : int
        {
            Normal = 0,
            Missing = 1,
            Null = 2,
        }
        public static AssetReferenceType CheckReference(UnityEngine.Object reference)
        {
            try
            {
                var blarf = reference.name;
            }
            catch (MissingReferenceException) // General Object like GameObject/Sprite etc
            {
                return AssetReferenceType.Missing;
            }
            catch (MissingComponentException) // Specific for objects of type Component
            {
                return AssetReferenceType.Missing;
            }
            catch (UnassignedReferenceException) // Specific for unassigned fields
            {
                return AssetReferenceType.Null;
            }
            catch (NullReferenceException) // Any other null reference like for local variables
            {
                return AssetReferenceType.Null;
            }

            return AssetReferenceType.Normal;
        }
    }
}
