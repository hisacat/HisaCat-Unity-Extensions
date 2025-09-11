using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using HisaCat.UnityExtensions;

namespace HisaCat.RealTimeOcclusionCulling
{
    [CustomEditor(typeof(RTOccluder))]
    [CanEditMultipleObjects]
    public class RTOccluderEditor : Editor
    {
        private SerializedProperty m_Bounds;
        private SerializedProperty m_Renderers;
        private SerializedProperty m_FacePortals;

        private void OnEnable()
        {
            this.m_Bounds = this.serializedObject.FindProperty(nameof(this.m_Bounds));
            this.m_Renderers = this.serializedObject.FindProperty(nameof(this.m_Renderers));
            this.m_FacePortals = this.serializedObject.FindProperty(nameof(this.m_FacePortals));
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            RTOcclusionEditorUtility.InspectorGUI.DrawRenderersGUI(this.serializedObject, this.m_Renderers);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            RTOcclusionEditorUtility.InspectorGUI.DrawBoundsGUI(this.m_Bounds);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            RTOcclusionEditorUtility.InspectorGUI.DrawDebugGUI();

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Portal Management", EditorStyles.boldLabel);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(this.m_FacePortals);

            // Add portal, Clear portal ...

            this.serializedObject.ApplyModifiedProperties();
        }

        #region Gizmos
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void OnDrawGizmos(RTOccluder target, GizmoType gizmoType)
        {
            if (RTOcclusionEditorSettings.ShowGizmosAlways)
                DrawDefaultGizmos(target);
        }
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void OnDrawGizmosSelected(RTOccluder target, GizmoType gizmoType)
        {
            if (RTOcclusionEditorSettings.ShowGizmosAlways == false)
                DrawDefaultGizmos(target);

            RTOcclusionEditorUtility.DrawGizmos.DrawOverlappedCells(target);
        }
        private static void DrawDefaultGizmos(RTOccluder target)
        {
            // Set matrix
            // Gizmos.matrix = Matrix4x4.TRS(target.transform.position, target.transform.rotation, target.transform.lossyScale);
            Gizmos.matrix = target.transform.localToWorldMatrix;
            {
                // Draw bounds
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(target.Bounds.center, target.Bounds.size);

                // Draw portals
                foreach (var facePortal in target.FacePortals)
                {
                    if (facePortal == null) continue;
                    Gizmos.color = facePortal.IsEnabled ? Color.cyan.SetAlpha(0.25f) : Color.gray.SetAlpha(0.25f);
                    var bounds = facePortal.GetBounds(target);
                    Gizmos.DrawCube(bounds.center, bounds.size);
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
    #endregion Gizmos
}
