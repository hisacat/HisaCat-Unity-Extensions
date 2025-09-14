#if UNITY_EDITOR && FISHNET
using FishNet.Object;
using HisaCat.UnityExtensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HisaCat.FishNet.BuildProcessor
{
    public class EditorOnlyNetworkObjectBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var scenes = EditorBuildSettings.scenes;
            int findCount = 0;

            foreach (var sceneSetting in scenes)
            {
                if (!sceneSetting.enabled) continue;

                string scenePath = sceneSetting.path;
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                bool needCloseScene = false;
                if (!scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    needCloseScene = true;
                }

                foreach (var rootObj in scene.GetRootGameObjects())
                {
                    foreach (var netObj in rootObj.GetComponentsInChildren<NetworkObject>(true))
                    {
                        if (HasEditorOnlyTag(netObj.gameObject))
                        {
                            Debug.LogWarning($"EditorOnly {nameof(NetworkObject)} detected in scene \"{scenePath}\" (path: {netObj.transform.GetFullPath()})");
                            findCount++;
                        }
                    }
                }

                if (needCloseScene)
                    EditorSceneManager.CloseScene(scene, true);
            }

            if (findCount > 0)
            {
                var goOn = EditorUtility.DisplayDialog(
                    "EditorOnly NetworkObject Detected",
                    $"{findCount} of EditorOnly {nameof(NetworkObject)} detected."
                    + "\r\nIt may not be synchronized properly between build and UnityEditor."
                    + "\r\nContinue the build?",
                    "Yes", "No");

                if (goOn == false)
                    throw new BuildFailedException("User canceled the build from EditorOnly NetworkObject warning.");
            }
        }

        private bool HasEditorOnlyTag(GameObject obj)
        {
            while (obj != null)
            {
                if (obj.CompareTag("EditorOnly"))
                    return true;
                obj = obj.transform.parent ? obj.transform.parent.gameObject : null;
            }
            return false;
        }
    }
}
#endif
