using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HisaCat
{
    public class IOAsyncUtil
    {
        public static IEnumerator WriteFileAsyncRoutine(string filePath, byte[] bytes)
        {
            var writeTask = WriteFileAsync(filePath, bytes);
            yield return new WaitUntil(() => writeTask.IsCompleted);
        }

        public static async Task WriteFileAsync(string filePath, byte[] bytes)
        {
            using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Create))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}
