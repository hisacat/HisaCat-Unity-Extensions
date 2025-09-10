#if BAKERY_INCLUDED
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HisaCat.Bakery
{
    public static class BakeryExtensions
    {
        public static class Lightmaps
        {
            /// <summary>
            /// 모든 Scene의 Lightmap을 새로고침합니다.<br/>
            /// FishNet 환경에서 Lightmap이 Bake된 Prefab을 비동기 생성할 때 발생하는 Lightmap 로드 문제를 해결합니다.<br/>
            /// <see cref="ftLightmaps.RefreshFull"/>가 동작하지 않을 때 사용하세요.<br/>
            /// 모든 Scene의 <see cref="ftLightmapsStorage"/>를 찾아 <see cref="ftLightmaps.RefreshScene"/>으로 Lightmap을 새로고침합니다.
            /// </summary>
            public static void RefreshFullManually()
            {
                // Cache origin activeScene.
                var activeScene = SceneManager.GetActiveScene();
                var sceneCount = SceneManager.sceneCount;

                // Create new LightmapData array.
                for (int i = 0; i < sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded) continue;
                    SceneManager.SetActiveScene(scene);
                    LightmapSettings.lightmaps = new LightmapData[0];
                }

                // Refresh Lightmap for each scene.
                for (int sceneIdx = 0; sceneIdx < sceneCount; sceneIdx++)
                {
                    var scene = SceneManager.GetSceneAt(sceneIdx);
                    var storages = GetLightmapsStoragesInScene(scene);

                    static List<ftLightmapsStorage> GetLightmapsStoragesInScene(Scene scene)
                    {
                        var result = new List<ftLightmapsStorage>();

                        var rootGameObjects = scene.GetRootGameObjects();
                        var count = rootGameObjects.Length;
                        for (int i = 0; i < count; i++)
                        {
                            var rootGameObject = rootGameObjects[i];
                            var storages = rootGameObject.GetComponentsInChildren<ftLightmapsStorage>(includeInactive: true);
                            result.AddRange(storages);
                        }
                        return result;
                    }

                    var storageCount = storages.Count;
                    for (int i = 0; i < storageCount; i++)
                    {
                        var storage = storages[i];
                        ftLightmaps.RefreshScene(scene, storage, true);
                    }
                }

                // Reset ActiveScene as origin activeScene.
                SceneManager.SetActiveScene(activeScene);
            }
        }
    }
}
#endif
