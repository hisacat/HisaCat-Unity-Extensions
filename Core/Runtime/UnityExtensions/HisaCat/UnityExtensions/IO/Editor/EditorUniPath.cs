using System;
using UnityEngine;
using Path = System.IO.Path;
#if UNITY_EDITOR

namespace HisaCat.IO
{
    public static class EditorUniPath
    {
        public static string GetCurrentScriptAbsolutePath()
        {
            var relativePath = new System.Diagnostics.StackTrace(true).GetFrame(1).GetFileName();
            var absolutePath = UniPath.NormalizePath(Path.GetFullPath(relativePath));
            return absolutePath;
        }

        public static string GetCurrentScriptAssetPath()
        {
            var absolutePath = GetCurrentScriptAbsolutePath();
            var projectRootPath = UniPath.GetProjectRootPath();
            
            if (absolutePath.StartsWith(projectRootPath, StringComparison.OrdinalIgnoreCase) == false)
            {
                Debug.LogError($"[{nameof(EditorUniPath)}] Current script path is not within the project path. absolute path will be returned: \"{absolutePath}\"");
                return absolutePath;
            }

            var assetPath = absolutePath.Substring(projectRootPath.Length).TrimStart('/');
            return assetPath;
        }
    }
}
#endif
