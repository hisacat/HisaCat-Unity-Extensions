using UnityEditor;

/*
* Copyright 2016, HisaCat https://github.com/hisacat
* Released under the MIT license
*/
namespace FastSceneChanger
{
    public class ScenePostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            int count;
            bool IsSceneAsset(string assetPath)
                => System.IO.Path.GetExtension(assetPath).ToLower().Equals(".unity", System.StringComparison.OrdinalIgnoreCase);

            count = importedAssets.Length;
            for (int i = 0; i < count; i++)
            {
                var assetPath = importedAssets[i];
                if (IsSceneAsset(assetPath))
                {
                    FastSceneChangerEditorWindow fastSceneChanger = FastSceneChangerEditorWindow.GetWindowIsisOpen();
                    if (fastSceneChanger != null)
                    {
                        fastSceneChanger.SceneAdded(assetPath);
                    }
                }
            }

            count = deletedAssets.Length;
            for (int i = 0; i < count; i++)
            {
                var assetPath = deletedAssets[i];
                if (IsSceneAsset(assetPath))
                {
                    FastSceneChangerEditorWindow fastSceneChanger = FastSceneChangerEditorWindow.GetWindowIsisOpen();
                    if (fastSceneChanger != null)
                    {
                        fastSceneChanger.SceneRemoved(assetPath);
                    }
                }
            }

            count = movedAssets.Length;
            for (int i = 0; i < count; i++)
            {
                var assetPath = movedAssets[i];
                var from = movedFromAssetPaths[i];

                if (IsSceneAsset(assetPath))
                {
                    FastSceneChangerEditorWindow fastSceneChanger = FastSceneChangerEditorWindow.GetWindowIsisOpen();
                    if (fastSceneChanger != null)
                    {
                        fastSceneChanger.SceneRemoved(from);
                        fastSceneChanger.SceneAdded(assetPath);
                    }
                }
            }

        }
    }
}
