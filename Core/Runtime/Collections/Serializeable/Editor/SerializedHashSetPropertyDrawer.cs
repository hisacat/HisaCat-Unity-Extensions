#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HisaCat.HUE.Collections
{
    [CustomPropertyDrawer(typeof(SerializedHashSet<>))]
    public class SerializedHashSetPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                var itemsProperty = property.FindPropertyRelative("_items");
                if (itemsProperty == null)
                {
                    EditorGUI.LabelField(position, label, new GUIContent($"{typeof(SerializedHashSet<>).Name} configuration error."));
                    EditorGUI.EndProperty();
                    return;
                }

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.PropertyField(position, itemsProperty, label, true);
                }
                if (EditorGUI.EndChangeCheck())
                {

                }
            }
            EditorGUI.EndProperty();
            return;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty != null)
            {
                return EditorGUI.GetPropertyHeight(itemsProperty, label, true);
            }
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
