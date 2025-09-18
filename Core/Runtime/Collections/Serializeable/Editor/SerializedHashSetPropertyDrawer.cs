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

                var prevArraySize = itemsProperty.arraySize;
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.PropertyField(position, itemsProperty, label, true);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    // When add button is pressed, replace duplicated items with default values for better UX
                    var currenArraySize = itemsProperty.arraySize;
                    if (currenArraySize > prevArraySize)
                    {
                        var addedElement = itemsProperty.GetArrayElementAtIndex(currenArraySize - 1);
                        var newValue = GetValue(addedElement);
                        for (int i = 0; i < currenArraySize - 1; i++)
                        {
                            if (GetValue(itemsProperty.GetArrayElementAtIndex(i)) == newValue)
                            {
                                SetDefaultValue(addedElement);
                                break;
                            }
                        }
                    }
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

        private object GetValue(SerializedProperty property)
        {
            // For other types, reset to default
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    Debug.LogWarning("Generic SerializedProperty not supported");
                    return null;
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.intValue;
                case SerializedPropertyType.Character:
                    return property.stringValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Gradient:
                    return property.gradientValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    Debug.LogWarning("FixedBufferSize SerializedProperty not supported");
                    return null;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceValue;
                case SerializedPropertyType.Hash128:
                    return property.hash128Value;
                case SerializedPropertyType.RenderingLayerMask:
                    return property.intValue;
                default:
                    Debug.LogWarning($"SerializedPropertyType {property.propertyType} not supported");
                    return null;
            }
        }
        private void SetDefaultValue(SerializedProperty property)
        {
            // For other types, reset to default
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    Debug.LogWarning("Generic SerializedProperty not supported");
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = default;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = default;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = default;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = default;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = default;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = default;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = default;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = default;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = default;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = default;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Character:
                    property.stringValue = default;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = default;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = default;
                    break;
                case SerializedPropertyType.Gradient:
                    property.gradientValue = default;
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = default;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = default;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    Debug.LogWarning("FixedBufferSize SerializedProperty not supported");
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = default;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = default;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = default;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = default;
                    break;
                case SerializedPropertyType.Hash128:
                    property.hash128Value = default;
                    break;
                case SerializedPropertyType.RenderingLayerMask:
                    property.intValue = default;
                    break;
                default:
                    Debug.LogWarning($"SerializedPropertyType {property.propertyType} not supported");
                    break;
            }
        }
    }
}
#endif
