#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace HisaCat.Editors
{
    public static class RemoveGradleCaches
    {
        [MenuItem("HisaCat/Gradle/Remove Gradle Caches")]
        public static void Work()
        {
            var cachesPath = System.IO.Path.Join("%USERPROFILE%", ".gradle", "caches");
            cachesPath = System.Environment.ExpandEnvironmentVariables(cachesPath);
            if (System.IO.Directory.Exists(cachesPath) == false)
            {
                EditorUtility.DisplayDialog("Remove Gradle Caches", $"Cannot find directory \"{cachesPath}\"", "Ok");
                return;
            }

            try
            {
                System.IO.Directory.Delete(cachesPath, true);
                EditorUtility.DisplayDialog("Remove Gradle Caches", $"Directory \"{cachesPath}\" Removed.", "Ok");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Remove Gradle Caches", $"Directory \"{cachesPath}\" Remove failed!\nOpenJDK is alive?", "Ok");
            }
        }
    }
}
#endif
