using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using HisaCat.UnityExtensions;
using System.Linq;

namespace HisaCat.Tools
{
    public static class GameObjectNameTools
    {
        private const string RevertToPrefabNameMenuPath = "GameObject/HisaCat/NameTools/Revert To Prefab Name";
        [MenuItem(RevertToPrefabNameMenuPath, true, 0)]
        private static bool RevertToPrefabNameValidate()
        {
            if (Application.isPlaying) return false;
            return Selection.gameObjects.Length > 0;
        }
        [MenuItem(RevertToPrefabNameMenuPath, false, 0)]
        private static void RevertToPrefabName(MenuCommand command)
        {
            // Block function call multiple times when multiple object selected.
            if (command.context != null && Selection.gameObjects.Length > 1 && Selection.gameObjects[0] != command.context)
                return;

            var targets = Selection.gameObjects;
            foreach (var target in targets)
            {
                if (target == null) continue;

                var prefabType = PrefabUtility.GetPrefabAssetType(target);
                switch (prefabType)
                {
                    // PASS.
                    case PrefabAssetType.Model:
                    case PrefabAssetType.Regular:
                    case PrefabAssetType.Variant:
                        break;
                    default:
                        continue;
                }

                // target이 Prefab의 root가 아닌 "일부"라면 아무것도 하지 않음 (continue)
                var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(target);
                if (prefabRoot != target) continue;

                // target의 prefab asset을 가져와서, target의 이름을 Prefab의 이름으로 변경
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(target);
                if (prefabAsset == null) continue;

                Undo.RecordObject(target, "Revert To Prefab Name");
                target.name = prefabAsset.name;
            }
        }

        private const string ToUniqueNameMenuPath = "GameObject/HisaCat/NameTools/To Unique Name";
        [MenuItem(ToUniqueNameMenuPath, true, 0)]
        private static bool ToUniqueNameValidate()
        {
            if (Application.isPlaying) return false;
            return Selection.gameObjects.Length > 0;
        }
        [MenuItem(ToUniqueNameMenuPath, false, 0)]
        private static void ToUniqueName(MenuCommand command)
        {
            // Block function call multiple times when multiple object selected.
            if (command.context != null && Selection.gameObjects.Length > 1 && Selection.gameObjects[0] != command.context)
                return;

            var targets = Selection.gameObjects;
            targets = targets.Where(e => e != null)
                .OrderBy(e => e.transform.parent == null ? -1 : e.transform.parent.GetInstanceID())
                .ThenBy(e => e.transform.GetSiblingIndex()).ToArray();
            var leftTargets = new HashSet<GameObject>(targets);
            foreach (var target in targets)
            {
                leftTargets.Remove(target);

                var parent = target.transform.parent;
                GameObject[] siblings;
                if (parent == null)
                    siblings = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                else
                    siblings = parent.GetChildrenArray().Select(e => e.gameObject).ToArray();

                var siblingNames = new HashSet<string>();
                foreach (var sibling in siblings)
                {
                    if (leftTargets.Contains(sibling)) continue;
                    if (sibling == target) continue;
                    siblingNames.Add(sibling.name);
                }

                string baseName = target.name;
                string uniqueName = baseName;
                int index = 1;
                while (siblingNames.Contains(uniqueName))
                {
                    uniqueName = $"{baseName} ({index})";
                    index++;
                }

                if (uniqueName != target.name)
                {
                    Undo.RecordObject(target, "To Unique Name");
                    target.name = uniqueName;
                }
            }
        }
    }
}
