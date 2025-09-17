using System;
using UnityEngine;

using Path = System.IO.Path;

namespace HisaCat.IO
{
    public static class UniPath
    {
        public static string GetProjectRootPath()
        {
            var dataPath = NormalizePath(Application.dataPath);
            var projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
            return projectRoot;
        }

        public static string Combine(string path1, string path2, string path3, string path4)
            => NormalizePath(Path.Combine(path1, path2, path3, path4));
        public static string Combine(params string[] paths)
            => NormalizePath(Path.Combine(paths));
        public static string Combine(string path1, string path2)
            => NormalizePath(Path.Combine(path1, path2));
        public static string Combine(string path1, string path2, string path3)
            => NormalizePath(Path.Combine(path1, path2, path3));
        public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
            => NormalizePath(Path.Join(path1, path2));
        public static string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3)
            => NormalizePath(Path.Join(path1, path2, path3));

        public static string NormalizePath(string path)
            => path?.Replace('\\', '/');
    }
}
