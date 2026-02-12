namespace HisaCat.HUE.DataBindEx.Components.Presentation.Editors
{
    using HisaCat.HUE.DataBindEx.Editors;
    using HisaCat.UnityExtensions.Editors;
    using Slash.Unity.DataBind.Core.Presentation;
    using Slash.Unity.DataBind.Core.Utils;
    using Slash.Unity.DataBind.Editor.Utils;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(MasterType))]
    public class MasterTypeEditor : Editor
    {
        private SerializedProperty contextType = null;
        private SerializedProperty createContext = null;

        private void OnEnable()
        {
            this.contextType = this.serializedObject.FindProperty("contextType");
            this.createContext = this.serializedObject.FindProperty("createContext");
        }
        public override void OnInspectorGUI()
        {
            var castingContextType = string.IsNullOrEmpty(contextType.stringValue) ? null : ReflectionUtils.FindType(contextType.stringValue);
            var parentContextHolder = GetParentContextHolder();
            var parentContextType = parentContextHolder == null ? null : parentContextHolder.ContextType;
            var parentContextTypePath = parentContextHolder == null ? null : ContextTypeDrawerEx.FormatContextTypePath(parentContextType.FullName);

            EditorGUILayoutExtensions.ReadOnlyTextField("Parent Type", parentContextTypePath, expandLabelWidth: 3);
            EditorGUILayout.PropertyField(this.contextType, new GUIContent("Target Type"));
            EditorGUILayout.Space();

            if (Application.isPlaying == false)
            {
                if (castingContextType == null)
                {
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Target type not set");
                }
                else if (parentContextType == null)
                {
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Parent type not set");
                }
                else
                {
                    var parentToCurrentCastingable = castingContextType.IsAssignableFrom(parentContextType);
                    var currentToParentCastingable = parentContextType.IsAssignableFrom(castingContextType);
                    if (parentToCurrentCastingable == false && currentToParentCastingable == false)
                    {
                        EditorGUILayoutExtensions.ReadOnlyTextField("Status", "❌ Incompatible");
                    }
                    else
                    {
                        if (parentToCurrentCastingable)
                            EditorGUILayoutExtensions.ReadOnlyTextField("Status", "✔️ Compatible");
                        else
                            EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Unknown until runtime");
                    }
                }
            }
            else
            {
                var runtimeParentContext = parentContextHolder == null ? null : parentContextHolder.Context;
                var runtimeParentContextType = runtimeParentContext == null ? null : runtimeParentContext.GetType();
                var runtimeParentContextTypePath = runtimeParentContextType == null ? "null" : ContextTypeDrawerEx.FormatContextTypePath(runtimeParentContextType.FullName);
                EditorGUILayoutExtensions.ReadOnlyTextField("Runtime Parent Type", runtimeParentContextTypePath, expandLabelWidth: 3);


                var runtimeParentContextCastingable = castingContextType.IsAssignableFrom(runtimeParentContextType);
                if (runtimeParentContextCastingable)
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "✔️ Compatible");
                else
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "❌ Incompatible");
            }

            // createContext must be false always.
            if (this.createContext.boolValue) this.createContext.boolValue = false;

            this.serializedObject.ApplyModifiedProperties();
        }

        private ContextHolder GetParentContextHolder()
        {
            var component = this.target as Component;
            if (component == null) return null;
            var transform = component.transform;

            var parentContextHolder = (transform.parent == null)
                ? null : transform.parent.GetComponentInParent<ContextHolder>(includeInactive: true);

            return parentContextHolder;
        }
    }
}
