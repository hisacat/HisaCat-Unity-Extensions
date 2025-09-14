#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HisaCat.PropertyAttributes
{
    [CustomPropertyDrawer(typeof(RenderingLayerMaskAttribute))]
    public class RenderingLayerMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            uint value = (uint)property.longValue;
            value = (uint)EditorGUILayout.RenderingLayerMaskField(label, value);

            if (EditorGUI.EndChangeCheck())
            {
                property.longValue = value;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
