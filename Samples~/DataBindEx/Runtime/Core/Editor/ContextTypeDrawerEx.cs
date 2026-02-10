using Slash.Unity.DataBind.Editor.PropertyDrawers;
using Slash.Unity.DataBind.Core.Utils;
using Slash.Unity.DataBind.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.DataBindEx.Editors
{
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
            {
                var path = paths[i];

                // Replace + (indicating nested context classes) with / to create submenus.
                path = path.Replace("+", "/");

                // Replace `1, `2, ... (indicating generic types) with <>, <,>, ...
                {
                    var symbolIdx = path.IndexOf("`");
                    if (symbolIdx > 0)
                    {
                        var withoutSymbolPath = path.Substring(0, symbolIdx);
                        var genericTypeCountStr = path.Substring(symbolIdx + 1);
                        if (int.TryParse(genericTypeCountStr, out int genericTypeCount))
                        {
                            withoutSymbolPath += "<";
                            for (int j = 0; j < genericTypeCount - 1; j++)
                                withoutSymbolPath += ",";
                            withoutSymbolPath += ">";

                            path = withoutSymbolPath;
                        }
                    }
                }
                paths[i] = path;
            }
            return paths;
        }
    }
}
