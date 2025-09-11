using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HisaCat.UnityExtensions.Editors
{
    public static class FindAllMissingSpriteInImageComponent
    {
        [MenuItem("AssetHelper/Missing/Find All Missing Sprite in Image components In Project")]
        public static void LaunchInProjects()
        {
            var targets = AssetDataBaseHelper.GetAllComponentsInProject<Image>();
            foreach (var target in targets)
            {
                if (AssetDataBaseHelper.CheckReference(target.sprite) == AssetDataBaseHelper.AssetReferenceType.Missing)
                {
                    Debug.LogError($"{target.name} sprite missing", target);
                }
            }

            Debug.Log("Done");
        }

        [MenuItem("AssetHelper/Missing/Find All Missing Sprite in Image components In Scene")]
        public static void LaunchInScenes()
        {
            var targets = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Image>();
            foreach (var target in targets)
            {
                if (AssetDataBaseHelper.CheckReference(target.sprite) == AssetDataBaseHelper.AssetReferenceType.Missing)
                {
                    Debug.LogError($"{target.name} sprite missing", target);
                }
            }

            Debug.Log("Done");
        }
    }
}
