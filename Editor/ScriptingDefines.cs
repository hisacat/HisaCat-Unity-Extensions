using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace HisaCat
{
    internal static class ScriptingDefines
    {
        [InitializeOnLoadMethod]
        public static void AddDefineSymbols()
        {
            // var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            // var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var currentDefineList = currentDefines.Split(';').ToList();

            List<string> addedDefineList = new();
            var hueDefines = new string[] { "HISACAT_UNITY_EXTENSIONS" };

            foreach (var hueDefine in hueDefines)
            {
                if (currentDefineList.Contains(hueDefine)) continue;
                currentDefineList.Add(hueDefine);
                addedDefineList.Add(hueDefine);
            }

            currentDefines = string.Join(";", currentDefineList.ToArray());
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, currentDefines);

            if (addedDefineList.Count > 0)
            {
                Debug.Log(
                    $"{Information.GetDisplayName()} defines added within player settings."
                    + $"\r\n {string.Join("\r\n", addedDefineList.ToArray().Select(e => $"- {e}"))}");
            }
        }
    }
}
