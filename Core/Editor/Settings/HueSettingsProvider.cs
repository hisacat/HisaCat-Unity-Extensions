using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.Settings
{
    internal class HueSettingsProvider : MonoBehaviour
    {
        internal const string RootPath = "Project/HisaCat/HUE";
        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider()
        {
            // SettingsScope.Project: Project Settings
            // SettingsScope.Project: Preferences
            return new(RootPath, SettingsScope.Project)
            {
                label = "HUE: HisaCat's Unity Extensions",
                guiHandler = OnGUI,
                keywords = new HashSet<string> { "HisaCat", "HUE" }
            };
        }

        private static void OnGUI(string searchContext)
        {

            EditorGUI.BeginChangeCheck();
            var settings = HueSettings.Instance;
            {
                var originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 250f;
                {
                    var gizmosSettings = settings.GizmosSettings;
                    EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.LabelField("Mesh Collider", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        var meshColliderSettings = gizmosSettings.MeshColliderSettings;
                        {
                            meshColliderSettings.ShowNonConvexWithRenderer
                                = EditorGUILayout.Toggle("Show Non-Convex with Renderer",
                                    meshColliderSettings.ShowNonConvexWithRenderer);

                            EditorGUI.BeginDisabledGroup(meshColliderSettings.ShowNonConvexWithRenderer == false);
                            {
                                meshColliderSettings.NonConvexWithRendererColor
                                    = EditorGUILayout.ColorField("Non-Convex with Renderer Color",
                                        meshColliderSettings.NonConvexWithRendererColor);

                                if (IndentGUILayoutButton("Reset to Default"))
                                {
                                    meshColliderSettings.NonConvexWithRendererColor
                                        = HueSettings.GizmosSettingsData.MeshColliderSettingsData.DefaultNonConvexWithRendererColor;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;


                    EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    {
                        EditorGUILayout.LabelField("Editor Play Mode Start Scene", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        {
                            EditorGUI.BeginChangeCheck();
                            {
                                var editorPlayModeStartSceneSettings = gizmosSettings.EditorPlayModeStartSceneSettings;
                                editorPlayModeStartSceneSettings.Enable =
                                    EditorGUILayout.Toggle("Enable", editorPlayModeStartSceneSettings.Enable);

                                EditorGUILayout.Space();
                                editorPlayModeStartSceneSettings.UseFirstBuildScene =
                                    EditorGUILayout.Toggle("Use First Build Scene", editorPlayModeStartSceneSettings.UseFirstBuildScene);

                                EditorGUI.BeginDisabledGroup(editorPlayModeStartSceneSettings.UseFirstBuildScene);
                                {
                                    EditorGUI.BeginChangeCheck();
                                    var scenePath = editorPlayModeStartSceneSettings.StartScenePath;
                                    var sceneAsset = string.IsNullOrEmpty(scenePath) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                                    sceneAsset = EditorGUILayout.ObjectField("Start Scene Path", sceneAsset, typeof(SceneAsset), false) as SceneAsset;
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        editorPlayModeStartSceneSettings.StartScenePath =
                                           sceneAsset == null ? null : AssetDatabase.GetAssetPath(sceneAsset);
                                    }
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                            if (EditorGUI.EndChangeCheck()) EditorPlayModeStartSceneManager.UpdateStartScene();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
            if (EditorGUI.EndChangeCheck())
                HueSettings.Save();

            static bool IndentGUILayoutButton(string text, params GUILayoutOption[] options)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                var result = GUILayout.Button(text, options);
                EditorGUILayout.EndHorizontal();
                return result;
            }
        }
    }
}
