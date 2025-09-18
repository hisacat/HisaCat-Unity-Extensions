#if UNITY_EDITOR
using DG.DemiEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HisaCat.HUE.Collections
{
    [CustomPropertyDrawer(typeof(SerializedHashSet<>))]
    public class SerializedHashSetPropertyDrawer : PropertyDrawer
    {
        private Dictionary<string, ReorderableList> reorderableListDict = new Dictionary<string, ReorderableList>();

        private ReorderableList GetReorderableList(SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null) return null;

            var propertyPath = property.propertyPath;
            if (!reorderableListDict.TryGetValue(propertyPath, out var reorderableList))
            {
                reorderableList = new ReorderableList(property.serializedObject, itemsProperty,
                    draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true);

                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, label);
                };

                reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = itemsProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, GUIContent.none);
                };

                // Custom add button behavior - always add null/default
                reorderableList.onAddCallback = list =>
                {
                    itemsProperty.arraySize++;
                    var newElement = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);

                    SetDefaultValue(newElement);

                    property.serializedObject.ApplyModifiedProperties();
                };

                reorderableListDict[propertyPath] = reorderableList;
            }

            return reorderableList;
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "SerializedHashSet configuration error.");
                EditorGUI.EndProperty();
                return;
            }

            var list = GetReorderableList(property, label);
            if (list != null)
            {
                list.DoList(position);
            }
            else
            {
                EditorGUI.PropertyField(position, itemsProperty, label, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var itemsProperty = property.FindPropertyRelative("_items");
            if (itemsProperty == null) return EditorGUIUtility.singleLineHeight;

            var list = GetReorderableList(property, label);
            if (list != null)
            {
                return list.GetHeight();
            }

            return EditorGUI.GetPropertyHeight(itemsProperty, label, true);
        }
    }
}
#endif
