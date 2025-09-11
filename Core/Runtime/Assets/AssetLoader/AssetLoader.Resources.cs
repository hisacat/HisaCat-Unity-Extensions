using UnityEngine;

using UnityResources = UnityEngine.Resources;

namespace HisaCat.HUE.Assets
{
    public static partial class AssetLoader
    {
        public static class Resources
        {
            public static T LoadSync<T>(string path) where T : Object
            {
                var asset = UnityResources.Load<T>(path);
                return asset;
            }
            public static ResourceRequest LoadAsync<T>(string path) where T : Object
            {
                var req = UnityResources.LoadAsync<T>(path);
                return req;
            }
            public static ResourceRequest LoadAsync<T>(string path, System.Action<T> completed) where T : Object
            {
                var req = LoadAsync<T>(path);
                req.completed += onCompleted;
                void onCompleted(AsyncOperation request)
                {
                    completed.Invoke(req.asset as T);
                    req.completed -= onCompleted;
                }
                return req;
            }
        }
    }
}
