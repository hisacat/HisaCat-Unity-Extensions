using UnityEngine;
using UnityEditor;
using HisaCat.UnityExtensions.Editors;

namespace HisaCat.HUE.UI.Windows
{
    [CustomEditor(typeof(WindowBase), true)]
    public class WindowBaseEditor : Editor
    {
        private SerializedProperty m_FocusOnlyInteractable = null;
        private SerializedProperty m_UnblocksRaycastsOnStartClose = null;
        private SerializedProperty m_AlwaysOnTop = null;
        private SerializedProperty m_EntrySelectable = null;
        private SerializedProperty m_AutoSelectEntrySelectable = null;
        private SerializedProperty m_BGMClip = null;
        private SerializedProperty m_PauseBGMOnShow = null;
        private SerializedProperty m_PauseBGMOnFocus = null;
        private SerializedProperty m_ShowSEClip = null;
        private SerializedProperty m_CloseSEClip = null;
        private SerializedProperty m_ShowAnimationClip = null;
        private SerializedProperty m_CloseAnimationClip = null;
        void OnEnable()
        {
            this.m_FocusOnlyInteractable = serializedObject.FindProperty(nameof(m_FocusOnlyInteractable));
            this.m_UnblocksRaycastsOnStartClose = serializedObject.FindProperty(nameof(m_UnblocksRaycastsOnStartClose));
            this.m_AlwaysOnTop = serializedObject.FindProperty(nameof(m_AlwaysOnTop));
            this.m_EntrySelectable = serializedObject.FindProperty(nameof(m_EntrySelectable));
            this.m_AutoSelectEntrySelectable = serializedObject.FindProperty(nameof(m_AutoSelectEntrySelectable));
            this.m_BGMClip = serializedObject.FindProperty(nameof(m_BGMClip));
            this.m_PauseBGMOnShow = serializedObject.FindProperty(nameof(m_PauseBGMOnShow));
            this.m_PauseBGMOnFocus = serializedObject.FindProperty(nameof(m_PauseBGMOnFocus));
            this.m_ShowSEClip = serializedObject.FindProperty(nameof(m_ShowSEClip));
            this.m_CloseSEClip = serializedObject.FindProperty(nameof(m_CloseSEClip));
            this.m_ShowAnimationClip = serializedObject.FindProperty(nameof(m_ShowAnimationClip));
            this.m_CloseAnimationClip = serializedObject.FindProperty(nameof(m_CloseAnimationClip));

        }

        public void DrawBaseInspector()
        {
            serializedObject.UpdateIfRequiredOrScript();

            this.DrawScriptField();

            EditorGUILayout.PropertyField(this.m_FocusOnlyInteractable);
            EditorGUILayout.HelpBox(
                "When enabled, the Interactable value of the CanvasGroup is"
                + "\r\nautomatically set based on the focus state.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.m_UnblocksRaycastsOnStartClose);
            EditorGUILayout.HelpBox(
                "When enabled, the CanvasGroup.blocksRaycasts is set to false"
                + "\r\nwhen the window is closed.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.m_AlwaysOnTop);
            EditorGUILayout.HelpBox(
                "Keeps the window always on top."
                + "\r\n(This does not affect control based on the actual focused state.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Entry Selectable", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(this.m_EntrySelectable);
                EditorGUILayout.PropertyField(this.m_AutoSelectEntrySelectable);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("BGM", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(this.m_BGMClip);
                EditorGUILayout.LabelField("Pause BGM On ...");
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                {
                    this.m_PauseBGMOnShow.boolValue = EditorGUILayout.ToggleLeft("Show", this.m_PauseBGMOnShow.boolValue, GUILayout.MaxWidth(100));
                    this.m_PauseBGMOnFocus.boolValue = EditorGUILayout.ToggleLeft("Focus", this.m_PauseBGMOnFocus.boolValue, GUILayout.MaxWidth(100));
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("SE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(this.m_ShowSEClip);
                EditorGUILayout.PropertyField(this.m_CloseSEClip);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(this.m_ShowAnimationClip);
                EditorGUILayout.PropertyField(this.m_CloseAnimationClip);
            }
            EditorGUI.indentLevel--;


            #region Validate AnimationClip
            // Display a warning if the AnimationClip contains a key for
            // Interactable or Blocks Raycasts of the CanvasGroup on the root object.
            bool hasWarningKey = false;
            AnimationClip showClip = this.m_ShowAnimationClip.objectReferenceValue as AnimationClip;
            AnimationClip closeClip = this.m_CloseAnimationClip.objectReferenceValue as AnimationClip;
            if (showClip != null || closeClip != null)
            {
                AnimationClip[] clips = new AnimationClip[] { showClip, closeClip };
                foreach (var clip in clips)
                {
                    if (clip == null) continue;
                    var bindings = AnimationUtility.GetCurveBindings(clip);
                    foreach (var binding in bindings)
                    {
                        // 경로가 ""이고, 속성명이 Interactable 또는 BlocksRaycasts인지 확인
                        if (binding.path == "")
                        {
                            if (binding.propertyName == "m_Interactable" || binding.propertyName == "m_BlocksRaycasts")
                            {
                                hasWarningKey = true;
                                break;
                            }
                        }
                    }
                    if (hasWarningKey) break;
                }
            }

            EditorGUILayout.Space();
            if (hasWarningKey)
            {
                EditorGUILayout.HelpBox(
                    "It is not recommended to manually set the 'Interactable' or 'Blocks Raycasts' properties of the Canvas Group through animations or similar means."
                    + $"\r\n{nameof(WindowBase)} script will handle these properties automatically.", MessageType.Warning);
            }
            #endregion Validate AnimationClip

            serializedObject.ApplyModifiedProperties();
        }

        override public void OnInspectorGUI()
        {
            DrawBaseInspector();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.Space();
            DrawPropertiesExcluding(this.serializedObject, new[] {
                "m_Script",
                nameof(this.m_FocusOnlyInteractable),
                nameof(this.m_UnblocksRaycastsOnStartClose),
                nameof(this.m_AlwaysOnTop),
                nameof(this.m_EntrySelectable),
                nameof(this.m_AutoSelectEntrySelectable),
                nameof(this.m_BGMClip),
                nameof(this.m_PauseBGMOnShow),
                nameof(this.m_PauseBGMOnFocus),
                nameof(this.m_ShowSEClip),
                nameof(this.m_CloseSEClip),
                nameof(this.m_ShowAnimationClip),
                nameof(this.m_CloseAnimationClip),
            });

            serializedObject.ApplyModifiedProperties();
        }
    }
}