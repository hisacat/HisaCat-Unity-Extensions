using Slash.Unity.DataBind.Editor.PropertyDrawers;

namespace HisaCat.HUE.DataBindEx.Editors
{
    using Slash.Unity.DataBind.Core.Utils;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ContextPathAttribute))]
    public class PathPropertyDrawerEx : PathPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //For block multiple objects
            if (property.serializedObject.isEditingMultipleObjects)
            {
                var contextPathAttribute = this.attribute as ContextPathAttribute;
                var pathDisplayName =
                    contextPathAttribute != null && !string.IsNullOrEmpty(contextPathAttribute.PathDisplayName)
                        ? contextPathAttribute.PathDisplayName
                        : "Path";

                EditorGUI.LabelField(position, pathDisplayName, "Multiple", EditorStyles.boldLabel);
                return;
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }
    }
}
