#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using ColorUtility = UnityEngine.ColorUtility;
using HisaCat.HUE.UnityExtensions;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static class RTOcclusionEditorUtility
    {
        public static class InspectorGUI
        {
            public static void DrawDebugGUI()
            {
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        RTOcclusionEditorSettings.ShowGizmosAlways = EditorGUILayout.Toggle("Show Gizmos Always", RTOcclusionEditorSettings.ShowGizmosAlways);
                        RTOcclusionEditorSettings.ShowSelectedOverlappedCells = EditorGUILayout.Toggle("Show Selected Overlapped Cells", RTOcclusionEditorSettings.ShowSelectedOverlappedCells);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Repaint scene views when draw gizmos status is changed.
                        SceneView.RepaintAll();
                    }
                }
                EditorGUI.indentLevel--;
            }

            public static void DrawStatusGUI()
            {
                EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.LabelField($"Detected occluders: {RTOcclusionManager.Occluders.Count}");
                    EditorGUILayout.LabelField($"Detected occludees: {RTOcclusionManager.Occludees.Count}");
                    EditorGUILayout.LabelField($"{RTOcclusionManager.CulledOccludees.Count} occludees culled");
                }
                EditorGUI.indentLevel--;
            }

            public static void DrawRenderersGUI(SerializedObject so, SerializedProperty renderers)
            {
                EditorGUILayout.PropertyField(renderers);
                {
                    var set = new HashSet<Renderer>();
                    var duplicatesIndex = new List<int>();
                    var nullIndex = new List<int>();
                    int count = renderers.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        var element = renderers.GetArrayElementAtIndex(i);
                        var renderer = element.objectReferenceValue as Renderer;
                        if (renderer == null)
                        {
                            nullIndex.Add(i);
                            continue;
                        }

                        if (set.Add(renderer) == false)
                            duplicatesIndex.Add(i);
                    }

                    if (duplicatesIndex.Count > 0)
                    {
                        EditorGUILayout.HelpBox(
                            $"{duplicatesIndex.Count} of duplicated Renderers detected:"
                            , MessageType.Warning);
                        if (GUILayout.Button("Remove Duplicates"))
                        {
                            foreach (var index in duplicatesIndex.OrderByDescending(i => i))
                                renderers.DeleteArrayElementAtIndex(index);
                        }
                    }
                    if (nullIndex.Count > 0)
                    {
                        EditorGUILayout.HelpBox(
                            $"{nullIndex.Count} of null renderer detected."
                            , MessageType.Warning);
                        if (GUILayout.Button("Remove Null"))
                        {
                            foreach (var index in nullIndex.OrderByDescending(i => i))
                                renderers.DeleteArrayElementAtIndex(index);
                        }
                    }
                }

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                EditorGUILayout.LabelField("Renderer Management", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Refresh Renderers"))
                        UpdateRenderersAutomatically(so);

                    if (GUILayout.Button("Recalculate Bounds"))
                        UpdateBoundsAutomatically(so);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Refresh & Recalculate All", GUILayout.Height(25)))
                {
                    UpdateRenderersAutomatically(so);
                    UpdateBoundsAutomatically(so);
                }
            }

            public static void DrawBoundsGUI(SerializedProperty bounds)
            {
                EditorGUILayout.PropertyField(bounds);
                //EditorGUI.showMixedValue
            }

            private static void UpdateRenderersAutomatically(SerializedObject so)
            {
                foreach (var _target in so.targetObjects)
                {
                    var target = _target as RTOcclusionBase;
                    if (target == null) continue;

                    Undo.RecordObject(target, "Refresh Renderers");
                    target.UpdateRenderersAutomatically();
                    EditorUtility.SetDirty(target);
                }

                so.Update();
            }

            private static void UpdateBoundsAutomatically(SerializedObject so)
            {
                foreach (var _target in so.targetObjects)
                {
                    var target = _target as RTOcclusionBase;
                    if (target == null) continue;

                    Undo.RecordObject(target, "Recalculate Bounds");
                    target.UpdateBoundsAutomatically();
                    EditorUtility.SetDirty(target);
                }

                so.Update();
            }
        }

        public static class DrawGizmos
        {
            public static void DrawOverlappedCells(RTOcclusionBase target)
            {
                using (var enumerator = target.OverlappedCells.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var cell = enumerator.Current;
                        Gizmos.color = Color.cyan.WithAlpha(0.125f);
                        var center = new Vector3(cell.x, cell.y, cell.z) * RTOcclusionManager.CellSize;
                        center += (Vector3.one * RTOcclusionManager.CellSize) * 0.5f; // Center offset.
                        var size = new Vector3(RTOcclusionManager.CellSize, RTOcclusionManager.CellSize, RTOcclusionManager.CellSize);
                        Gizmos.DrawWireCube(center, size);
                        Gizmos.DrawCube(center, size);
                    }
                }
            }
        }

        public static class EditorPrefsExtensions
        {
            public static Color GetColor(string key, Color defaultValue)
            {
                var htmlString = EditorPrefs.GetString(key, defaultValue.ToString());
                if (ColorUtility.TryParseHtmlString(htmlString, out var color)) return color;
                return defaultValue;
            }

            public static void SetColor(string key, Color value)
            {
                EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGBA(value));
            }
        }
    }
}
#endif
