using HisaCat.HUE.UnityExtensions;
using HisaCat.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace HisaCat
{
    [DefaultExecutionOrder(int.MinValue)]
    public class CoroutineAction : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                _instance = null;
            }
        }
#pragma warning restore IDE0051
#endif

        private static CoroutineAction _instance = null;
        public static CoroutineAction Instance
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    Debug.LogError($"[{nameof(CoroutineAction)}] Instance is not available in editor mode.");
                    return null;
                }
#endif
                if (_instance == null)
                {
                    var go = new GameObject($"[{nameof(CoroutineAction)}]");
                    _instance = go.AddComponent<CoroutineAction>();
                }
                return _instance;
            }
        }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"[{nameof(CoroutineAction)}] Instance already exists!");
                Destroy(this.gameObject);
                return;
            }
            Init();
        }
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Dispose();
            }
        }

        private void Init() => DontDestroyOnLoad(this.gameObject);
        private void Dispose() { }

        public static Coroutine Start(IEnumerator enumerator)
        {
            return Instance.StartCoroutine(enumerator);
        }

        public static void Stop(Coroutine coroutine)
        {
            if (ApplicationUtils.IsQuitting()) return;
            Instance.StopCoroutine(coroutine);
        }

        public static void RestartCoroutine(ref Coroutine coroutine, IEnumerator enumerator)
            => Instance.RestartCoroutine(ref coroutine, enumerator);
        public static void StartOrStopCoroutine(ref Coroutine coroutine, IEnumerator enumerator, bool play, bool restartIfAlreadyPlaying)
            => Instance.StartOrStopCoroutine(ref coroutine, enumerator, play, restartIfAlreadyPlaying);

        public static void LoadImageFromURL(Texture2D texture, string url, Action<Texture2D> onLoaded)
        {
            Instance.StartCoroutine(LoadImageFromURLRoutine(texture, url, onLoaded));
        }

        public static IEnumerator LoadImageFromURLRoutine(Texture2D texture, string url, Action<Texture2D> onLoaded)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
                        if (downloadedTexture != null)
                        {
                            texture.Reinitialize(downloadedTexture.width, downloadedTexture.height);
                            texture.LoadRawTextureData(downloadedTexture.GetRawTextureData());
                            texture.Apply();

                            Destroy(downloadedTexture);
                        }
                    }
                    catch (Exception ex)
                    {
                        ManagedDebug.LogError($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLRoutine)}: Error loading texture: {ex.Message}");
                    }
                }
                else
                {
                    ManagedDebug.LogError($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLRoutine)}: Failed to load texture from \"{url}\", Error: {request.error}");
                }

                onLoaded?.Invoke(texture);
            }
        }
        public static void LoadImageFromURLWithCache(string url, Action<Texture2D> onLoaded)
        {
            Instance.StartCoroutine(LoadImageFromURLWithCacheRoutine(url, onLoaded));
        }

        private const string FileCacheFolderName = "Images"; // Cache folder name.
        private const string FileCacheExtension = ".tmp"; // Cache file extension.
        public static string GetFileCacheFolderPath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, FileCacheFolderName);
        }

        public static IEnumerator LoadImageFromURLWithCacheRoutine(string url, Action<Texture2D> onLoaded)
        {
            string fileName = HashUtil.GetMD5Hash(url).ToString() + FileCacheExtension;
            string folderPath = GetFileCacheFolderPath();
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            // Crate directory when its not exist.
            if (!System.IO.Directory.Exists(folderPath))
            {
                ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Cache folder does not exist.");
                System.IO.Directory.CreateDirectory(folderPath);
                ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Cache folder created at \"{folderPath}\"");
            }

            // Trying load cached texture.
            if (System.IO.File.Exists(filePath))
            {
                string pathForFile = "file://" + filePath;

                ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Trying load cache of \"{url}\" from \"{pathForFile}\".");
                using (UnityWebRequest loader = UnityWebRequestTexture.GetTexture(pathForFile))
                {
                    yield return loader.SendWebRequest();

                    if (string.IsNullOrEmpty(loader.error))
                    {
                        var texture = DownloadHandlerTexture.GetContent(loader);
                        if (texture != null)
                        {
                            ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Succeed to load from cache");
                            onLoaded?.Invoke(texture);
                            yield break;
                        }
                        else
                        {
                            ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Cache loaded but it broken. delete cache...");
                            System.IO.File.Delete(filePath);
                            ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Cache deleted");
                        }
                    }
                    else
                    {
                        ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Failed to load cache of \"{loader.uri}\". error: {loader.error}");
                    }
                }
            }

            ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Trying load texture from \"{url}\".");
            using (UnityWebRequest loader = UnityWebRequestTexture.GetTexture(url))
            {
                yield return loader.SendWebRequest();

                if (string.IsNullOrEmpty(loader.error))
                {
                    var texture = DownloadHandlerTexture.GetContent(loader);
                    if (texture != null)
                    {
                        ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Succeed to load from url");

                        ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Save cache file of \"{url}\" to \"{filePath}\"");
                        yield return IOAsyncUtil.WriteFileAsyncRoutine(filePath, texture.EncodeToPNG());
                        ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Cache file saved.");

                        onLoaded?.Invoke(texture);
                        yield break;
                    }
                }
                else
                {
                    ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Failed to load texture from \"{loader.uri}\". error: {loader.error}");
                }
            }

            ManagedDebug.Log($"[{nameof(CoroutineAction)}] {nameof(LoadImageFromURLWithCacheRoutine)}: Failed to load texture. return empty texture");

            var emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            emptyTexture.hideFlags = HideFlags.HideAndDontSave;
            emptyTexture.SetPixels32(new Color32[] { Color.clear });
            emptyTexture.Apply();

            onLoaded?.Invoke(emptyTexture);
        }

        public static void WaitFrame(int frameCount, Action callback)
        {
            Instance.StartCoroutine(WaitFrameRoutine(frameCount, callback));
            static IEnumerator WaitFrameRoutine(int frameCount, Action callback)
            {
                for (int i = 0; i < frameCount; i++)
                    yield return null;

                callback();
            }
        }
        public static void WaitOneFrame(Action callback)
        {
            Instance.StartCoroutine(WaitOneFrameRoutine(callback));
            static IEnumerator WaitOneFrameRoutine(Action callback)
            {
                yield return null;
                callback();
            }
        }

        public static void WaitSeconds(float seconds, Action callback)
        {
            Instance.StartCoroutine(Instance.WaitSecondsRoutine(seconds, callback));
        }
        private IEnumerator WaitSecondsRoutine(float seconds, Action callback)
        {
            yield return CachedYieldInstruction.WaitForSeconds(seconds);
            callback();
        }

        public static void WaitSecondsRealtime(float seconds, Action callback)
        {
            Instance.StartCoroutine(Instance.WaitSecondsRealtimeRoutine(seconds, callback));
        }
        private IEnumerator WaitSecondsRealtimeRoutine(float seconds, Action callback)
        {
            yield return CachedYieldInstruction.WaitForSecondsRealtime(seconds);
            callback();
        }

        public static void WaitForEndOfFrame(Action callback)
        {
            Instance.StartCoroutine(Instance.WaitForEndOfFrameRoutine(callback));
        }
        private IEnumerator WaitForEndOfFrameRoutine(Action callback)
        {
            yield return CachedYieldInstruction.WaitForEndOfFrame();
            callback();
        }

        public static void SimpleShake(GameObject obj, float power, float duration)
        {
            Instance.StartCoroutine(Instance.SimpleShakeRoutine(obj, power, duration));
        }
        private IEnumerator SimpleShakeRoutine(GameObject obj, float power, float duration)
        {
            var originPos = obj.transform.position;
            float curTime = 0;
            while (curTime <= duration)
            {
                obj.transform.position = originPos + (Vector3)(UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, power));
                curTime += Time.deltaTime;
                yield return null;
            }

            obj.transform.position = originPos;
        }
    }
}
