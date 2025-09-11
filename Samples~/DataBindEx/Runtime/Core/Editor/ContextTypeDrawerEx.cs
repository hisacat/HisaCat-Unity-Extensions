using Slash.Unity.DataBind.Editor.PropertyDrawers;

namespace HisaCat.HUE.DataBindEx.Editors
{
    using Slash.Unity.DataBind.Core.Utils;
    using Slash.Unity.DataBind.Editor.Utils;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ContextTypeAttribute))]
    public class ContextTypeDrawerEx : ContextTypeDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            // Find all available context classes.
            var contextTypes = ContextTypeCache.ContextTypes;
            var contextTypeIndex = string.IsNullOrEmpty(property.stringValue)
                ? 0
                : contextTypes.FindIndex(
                    contextType => contextType != null && contextType.AssemblyQualifiedName == property.stringValue);
            var newContextTypeIndex = EditorGUI.Popup(
                position,
                label.text,
                contextTypeIndex,
                ContextTypePathsFormatter(ContextTypeCache.ContextTypePaths));
            //ContextTypeCache.ContextTypePaths);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = contextTypes[newContextTypeIndex]?.AssemblyQualifiedName;
            }
            EditorGUI.EndProperty();
        }
        private string[] ContextTypePathsFormatter(string[] paths)
        {
            int count = paths.Length;
            for (int i = 0; i < count; i++)
                paths[i] = paths[i].Replace("+", "/");

            return paths;
        }
    }
}
