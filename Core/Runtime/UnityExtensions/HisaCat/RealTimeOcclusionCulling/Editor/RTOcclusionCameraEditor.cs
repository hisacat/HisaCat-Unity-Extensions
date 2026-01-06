#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HisaCat.UnityExtensions.Editors;

namespace HisaCat.RealTimeOcclusionCulling
{
    [CustomEditor(typeof(RTOcclusionCamera))]
    public class RTOcclusionCameraEditor : Editor
    {
        private void OnEnable() { }

        public override void OnInspectorGUI()
        {
            this.DrawScriptField();
            this.serializedObject.Update();

            RTOcclusionEditorUtility.InspectorGUI.DrawDebugGUI();
            RTOcclusionEditorUtility.InspectorGUI.DrawStatusGUI();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                GUI.color = RTOcclusionManager.IsPreviewMode ? Color.green : Color.white;
                if (GUILayout.Button(RTOcclusionManager.IsPreviewMode ? "End Preview Mode" : "Begin Preview Mode"))
                    RTOcclusionManager.IsPreviewMode = !RTOcclusionManager.IsPreviewMode;
                GUI.color = Color.white;
            }
            EditorGUI.EndDisabledGroup();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
