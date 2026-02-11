namespace HisaCat.HUE.DataBindEx.Components.Presentation.Editors
{
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
            var parentContextType = GetParentContextType();

            EditorGUILayoutExtensions.ReadOnlyTextField("Parent Type", (parentContextType == null ? "None" : parentContextType.FullName));
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.contextType, new GUIContent("Target Type"));

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
                var isCastingable = parentContextType.IsAssignableFrom(castingContextType);
                EditorGUILayoutExtensions.ReadOnlyTextField("Status", (isCastingable ? "✔️ Compatible" : "❌ Incompatible"));
            }

            // createContext must be false always.
            if (this.createContext.boolValue) this.createContext.boolValue = false;

            this.serializedObject.ApplyModifiedProperties();
        }

        private System.Type GetParentContextType()
        {
            var component = this.target as Component;
            if (component == null) return null;
            var transform = component.transform;

            var parentContextHolder = (transform.parent == null)
                ? null : transform.parent.GetComponentInParent<ContextHolder>(includeInactive: true);

            return parentContextHolder == null ? null : parentContextHolder.ContextType;
        }
    }
}
