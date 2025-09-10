using UnityEngine;
using UnityEditor;

namespace HisaCat.RealTimeOcclusionCulling
{
    [CustomEditor(typeof(RTOcclusionCamera))]
    public class RTOcclusionCameraEditor : Editor
    {
        private void OnEnable() { }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            RTOcclusionEditorUtility.InspectorGUI.DrawDebugGUI();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}