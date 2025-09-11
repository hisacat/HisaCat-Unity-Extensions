using System.Collections;
using UnityEngine;

namespace HisaCat
{
    public static class LocalFileSystem
    {
        #region Write
        public static string GetPath(string relativePath)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, relativePath);
        }
        private static string PrepareWrite(string relativePath)
        {
            var path = GetPath(relativePath);
            var rootDir = System.IO.Path.GetDirectoryName(path);

            // Create root dir.
            System.IO.Directory.CreateDirectory(rootDir);

            return path;
        }

        public static bool DeleteFile(string relativePath)
        {
            var path = GetPath(relativePath);
            try
            {
                System.IO.File.Delete(path);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        public static bool DeleteDirectory(string relativePath, bool recursive)
        {
            var path = GetPath(relativePath);
            try
            {
                System.IO.Directory.Delete(path, recursive);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool IsFileExists(string relativePath)
        {
            var path = PrepareWrite(relativePath);
            return System.IO.File.Exists(path);
        }
        public static void Copy(string sourceRelativePath, string destRelativePath, bool overwrite)
        {
            var sourcePath = PrepareWrite(sourceRelativePath);
            var destPath = PrepareWrite(destRelativePath);
            System.IO.File.Copy(sourcePath, destPath, overwrite);
        }

        public static void WriteAllText(string relativePath, string text)
            => WriteAllText(relativePath, text, System.Text.Encoding.UTF8);
        public static void WriteAllText(string relativePath, string text, System.Text.Encoding encoding)
        {
            var path = PrepareWrite(relativePath);
            System.IO.File.WriteAllText(path, text, encoding);
        }
        public static IEnumerator WriteAllTextRoutine(string relativePath, string text)
            => WriteAllTextRoutine(relativePath, text, System.Text.Encoding.UTF8);
        public static IEnumerator WriteAllTextRoutine(string relativePath, string text, System.Text.Encoding encoding)
        {
            var path = PrepareWrite(relativePath);
            var task = System.IO.File.WriteAllTextAsync(path, text, encoding);

            yield return new WaitUntil(() => task.IsCompleted);
        }
        public static void WriteAllBytes(string relativePath, byte[] bytes)
        {
            var path = PrepareWrite(relativePath);
            System.IO.File.WriteAllBytes(path, bytes);
        }
        public static IEnumerator WriteAllBytesRoutine(string relativePath, byte[] bytes)
        {
            var path = PrepareWrite(relativePath);
            var task = System.IO.File.WriteAllBytesAsync(path, bytes);

            yield return new WaitUntil(() => task.IsCompleted);
        }
        #endregion Write

        #region Read
        public static string ReadAllText(string relativePath)
        {
            relativePath = System.IO.Path.Combine(Application.persistentDataPath, relativePath);

            if (System.IO.File.Exists(relativePath) == false)
                return null;

            return System.IO.File.ReadAllText(relativePath);
        }
        public static IEnumerator ReadAllTextRoutine(string relativePath, System.Action<string> result)
        {
            relativePath = System.IO.Path.Combine(Application.persistentDataPath, relativePath);

            if (System.IO.File.Exists(relativePath) == false)
            {
                result.Invoke(null);
                yield break;
            }

            var task = System.IO.File.ReadAllTextAsync(relativePath);

            yield return new WaitUntil(() => task.IsCompleted);

            result.Invoke(task.Result);
            yield break;
        }
        public static byte[] ReadAllBytes(string relativePath)
        {
            relativePath = System.IO.Path.Combine(Application.persistentDataPath, relativePath);

            if (System.IO.File.Exists(relativePath) == false)
                return null;

            return System.IO.File.ReadAllBytes(relativePath);
        }
        public static IEnumerator ReadAllBytesRoutine(string relativePath, System.Action<byte[]> result)
        {
            relativePath = System.IO.Path.Combine(Application.persistentDataPath, relativePath);

            if (System.IO.File.Exists(relativePath) == false)
            {
                result.Invoke(null);
                yield break;
            }

            var task = System.IO.File.ReadAllBytesAsync(relativePath);

            yield return new WaitUntil(() => task.IsCompleted);

            result.Invoke(task.Result);
            yield break;
        }
        #endregion Read
    }
}
