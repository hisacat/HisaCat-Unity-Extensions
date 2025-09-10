using FishNet.Editing;
using FishNet.Object;
using FishNet.Object.Editing;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

#if FISHNET
namespace HisaCat.FishNet.Editor
{
    [CustomEditor(typeof(NetworkObject), true)]
    [CanEditMultipleObjects]
    public class NetworkObjectEditorExtension : NetworkObjectEditor
    {
        private static bool PropertyViewFoldout = false;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Network Object Editor Extension", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                PropertyViewFoldout = EditorGUILayout.Foldout(PropertyViewFoldout, "Property View", true);
                if (PropertyViewFoldout)
                {
                    EditorGUI.indentLevel++;
                    {
                        var curNob = this.target as NetworkObject;
                        var nobs = this.targets.Cast<NetworkObject>();

                        void DrawDisabledTextField(string name, System.Func<NetworkObject, string> getValue)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.showMixedValue = nobs.GroupBy(nob => getValue(nob)).Count() != 1;
                            EditorGUILayout.TextField(name, getValue(curNob));
                            EditorGUI.showMixedValue = false;
                            EditorGUI.EndDisabledGroup();
                        }
                        void DrawDisabledToggleField(string name, System.Func<NetworkObject, bool> getValue)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUI.showMixedValue = nobs.GroupBy(nob => getValue(nob)).Count() != 1;
                            EditorGUILayout.Toggle(name, getValue(curNob));
                            EditorGUI.showMixedValue = false;
                            EditorGUI.EndDisabledGroup();
                        }

                        DrawDisabledTextField(nameof(NetworkObject.ObjectId), nob => nob.ObjectId == NetworkObject.UNSET_OBJECTID_VALUE ? $"UNSET ({NetworkObject.UNSET_OBJECTID_VALUE})" : nob.ObjectId.ToString());
                        DrawDisabledTextField(nameof(NetworkObject.PrefabId), nob => nob.PrefabId == NetworkObject.UNSET_PREFABID_VALUE ? $"UNSET ({NetworkObject.UNSET_PREFABID_VALUE})" : nob.PrefabId.ToString());
                        DrawDisabledTextField(nameof(NetworkObject.OwnerId), nob => nob.OwnerId < 0 ? $"INVALID ({nob.OwnerId})" : nob.OwnerId.ToString());
                        DrawDisabledToggleField(nameof(NetworkObject.IsNested), nob => nob.IsNested);
                        DrawDisabledToggleField(nameof(NetworkObject.IsSpawned), nob => nob.IsSpawned);
                        DrawDisabledToggleField(nameof(NetworkObject.IsSceneObject), nob => nob.IsSceneObject);
                        EditorGUILayout.HelpBox(
                            "'IsSceneObject' indicates whether the internal 'SceneId' is set."
                            + "\nAdditionally, the 'SceneId' value is not stored in the Prefab itself, but in the Scene Asset that contains the object.",
                            MessageType.Info);
                        if (nobs.Any(e => e.IsSpawned == false && e.IsSceneObject == false && IsSceneInstance(e.gameObject)))
                        {
                            EditorGUILayout.HelpBox(
                                "One or more NetworkObjects are placed in the scene, but 'IsSceneObject' is false."
                                + "\nPlease Rebuild SceneIds with menu: 'Tools/Fish-Networking/Utility/Reserialize NetworkObjects'",
                                MessageType.Warning);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        private static bool IsSceneInstance(GameObject instance)
        {
            if (instance == null) return false;
            if (instance.scene.IsValid() == false) return false;
            if (string.IsNullOrEmpty(instance.scene.name)) return false;

            // Ignore prefab stage
            var prefabStage = PrefabStageUtility.GetPrefabStage(instance);
            if (prefabStage != null)
                if (instance.scene == prefabStage.scene) return false;

            return true;
        }
    }
}
#endif