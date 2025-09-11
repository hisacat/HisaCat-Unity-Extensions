using UnityEditor;
using UnityEngine;

namespace HisaCat.Editors
{
    [CustomPropertyDrawer(typeof(EnumStringAttribute))]
    public class EnumStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumStringAttribute enumString = (EnumStringAttribute)attribute;

            var enumNames = System.Enum.GetNames(enumString.EnumType);

            if (property.hasMultipleDifferentValues == false)
            {
                // Draw normally

                var originValue = property.stringValue;
                var idx = System.Array.IndexOf(enumNames, originValue);

                if (idx < 0) // Manage unknown value.
                {
                    // Resize array and add unknown value.
                    var newEnumNames = new string[enumNames.Length + 1];
                    System.Array.Copy(enumNames, newEnumNames, enumNames.Length);
                    newEnumNames[newEnumNames.Length - 1] = $"{originValue} (Unknown)";

                    EditorGUI.BeginChangeCheck();
                    var newIdx = EditorGUI.Popup(position, label.text, newEnumNames.Length - 1, newEnumNames);
                    if (EditorGUI.EndChangeCheck()) property.stringValue = enumNames[newIdx];
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    var newIdx = EditorGUI.Popup(position, label.text, idx, enumNames);
                    if (EditorGUI.EndChangeCheck()) property.stringValue = enumNames[newIdx];
                }
            }
            else
            {
                EditorGUI.showMixedValue = true;
                {
                    // Draw as multiple different field
                    EditorGUI.BeginChangeCheck();
                    var newIdx = EditorGUI.Popup(position, label.text, -1, enumNames);
                    if (EditorGUI.EndChangeCheck()) property.stringValue = enumNames[newIdx];
                }
                EditorGUI.showMixedValue = false;
            }

        }
    }
}
