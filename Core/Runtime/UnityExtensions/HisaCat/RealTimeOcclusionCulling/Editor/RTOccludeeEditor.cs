#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace HisaCat.RealTimeOcclusionCulling
{
    [CustomEditor(typeof(RTOccludee))]
    [CanEditMultipleObjects]
    public class RTOccludeeEditor : Editor
    {
        private SerializedProperty m_Bounds;
        private SerializedProperty m_Renderers; private SerializedProperty m_ShowBounds;

        private void OnEnable()
        {
            this.m_Bounds = this.serializedObject.FindProperty(nameof(this.m_Bounds));
            this.m_Renderers = this.serializedObject.FindProperty(nameof(this.m_Renderers));
            this.m_ShowBounds = this.serializedObject.FindProperty(nameof(this.m_ShowBounds));
        }

#pragma warning disable CS0414
        private static bool overrlappedCellsToggle = false; // TODO DISPLAY WITH TOGGLE
#pragma warning restore CS0414
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            RTOcclusionEditorUtility.InspectorGUI.DrawRenderersGUI(this.serializedObject, this.m_Renderers);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            RTOcclusionEditorUtility.InspectorGUI.DrawBoundsGUI(this.m_Bounds);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Culling Status", EditorStyles.boldLabel);
            {
                var target = this.target as RTOccludee;
                var targets = this.targets.Cast<RTOccludee>().ToArray();

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUI.showMixedValue = targets.GroupBy(t => (t as RTOccludee).IsCulled).Count() != 1;
                    EditorGUILayout.Toggle("Is Culled", target.IsCulled);
                    EditorGUI.showMixedValue = false;
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            RTOcclusionEditorUtility.InspectorGUI.DrawDebugGUI();

            this.serializedObject.ApplyModifiedProperties();
        }

        #region Gizmos
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        private static void OnDrawGizmos(RTOccludee target, GizmoType gizmoType)
        {
            if (RTOcclusionEditorSettings.ShowGizmosAlways)
                DrawDefaultGizmos(target);
        }
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void OnDrawGizmosSelected(RTOccludee target, GizmoType gizmoType)
        {
            if (RTOcclusionEditorSettings.ShowGizmosAlways == false)
                DrawDefaultGizmos(target);

            RTOcclusionEditorUtility.DrawGizmos.DrawOverlappedCells(target);
        }
        private static void DrawDefaultGizmos(RTOccludee target)
        {
            // Set matrix
            // Gizmos.matrix = Matrix4x4.TRS(target.transform.position, target.transform.rotation, target.transform.lossyScale);
            Gizmos.matrix = target.transform.localToWorldMatrix;
            {
                // Draw bounds
                Gizmos.color = target.IsCulled ? Color.orange : Color.green;
                Gizmos.DrawWireCube(target.Bounds.center, target.Bounds.size);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
        #endregion Gizmos
    }
}
#endif
