using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;

namespace HisaCat.UGUI
{
    public static class ScrollRectConverterEditor
    {
        [MenuItem("CONTEXT/ScrollRect/Invert Horizontal Scroll")]
        private static void InvertHorizontalScroll(MenuCommand command)
        {
            // TODO: 여러개 한번에 변환?
            ScrollRect originalScrollRect = (ScrollRect)command.context;
            GameObject gameObject = originalScrollRect.gameObject;

            Undo.RecordObject(gameObject, "Invert Horizontal Scroll");

            var originalNormalizedPosition = originalScrollRect.normalizedPosition;
            var originalComponentIndex = System.Array.IndexOf(gameObject.GetComponents<Component>(), originalScrollRect);

            SerializedObject originalSerialized = new SerializedObject(originalScrollRect);

            // Destroy the original ScrollRect component.
            Undo.DestroyObjectImmediate(originalScrollRect);

            // Add the new ScrollRect component.
            var newScrollRect =
                originalScrollRect.GetType() == typeof(HorizontalScrollInvertedScrollRect) ?
                    Undo.AddComponent<ScrollRect>(gameObject) :
                    Undo.AddComponent<HorizontalScrollInvertedScrollRect>(gameObject);

            SerializedObject newSerialized = new SerializedObject(newScrollRect);

            // Copy the properties from the original ScrollRect to the new ScrollRect.
            CopySerializedProperties(originalSerialized, newSerialized);

            // Apply the changes.
            newSerialized.ApplyModifiedProperties();

            // Restore the normalized position.
            newScrollRect.normalizedPosition = originalNormalizedPosition;

            // Move the new component to the original position
            MoveComponentToIndex(newScrollRect, originalComponentIndex);

            EditorUtility.SetDirty(gameObject);

            Debug.Log($"Successfully inverted horizontal scroll on {gameObject.name}");
        }

        [MenuItem("CONTEXT/ScrollRect/Invert Horizontal Scroll", true)]
        private static bool ValidateInvertHorizontalScroll(MenuCommand command)
        {
            var scrollRect = (ScrollRect)command.context;
            if (scrollRect == null) return false;

            var type = scrollRect.GetType();
            return type == typeof(ScrollRect) || type == typeof(HorizontalScrollInvertedScrollRect);
        }

        /// <summary>
        /// Move a component to a specific index in the component list.
        /// </summary>
        private static void MoveComponentToIndex(Component component, int targetIndex)
        {
            if (component == null) return;

            GameObject gameObject = component.gameObject;
            Component[] allComponents = gameObject.GetComponents<Component>();
            int currentIndex = System.Array.IndexOf(allComponents, component);

            // Calculate the actual target index (accounting for the deleted component).
            int actualTargetIndex = targetIndex;
            if (actualTargetIndex >= allComponents.Length)
                actualTargetIndex = allComponents.Length - 1;

            // Move the component up or down to reach the target position
            while (currentIndex > actualTargetIndex)
            {
                ComponentUtility.MoveComponentUp(component);
                currentIndex--;
            }

            while (currentIndex < actualTargetIndex)
            {
                ComponentUtility.MoveComponentDown(component);
                currentIndex++;
            }
        }

        /// <summary>
        /// Copy all matching serialized properties from source to target.
        /// </summary>
        private static void CopySerializedProperties(SerializedObject source, SerializedObject target)
        {
            SerializedProperty sourceProperty = source.GetIterator();

            // Enter children to iterate through all properties
            if (sourceProperty.NextVisible(true))
            {
                do
                {
                    // Skip m_Script property (component type reference)
                    if (sourceProperty.propertyPath == "m_Script")
                        continue;

                    // Find matching property in target.
                    SerializedProperty targetProperty = target.FindProperty(sourceProperty.propertyPath);

                    if (targetProperty != null && CanCopyProperty(sourceProperty, targetProperty))
                    {
                        // Copy the property value.
                        CopyPropertyValue(sourceProperty, targetProperty);
                    }
                }
                while (sourceProperty.NextVisible(false));
            }
        }

        /// <summary>
        /// Check if the property can be safely copied (same type and path)
        /// </summary>
        private static bool CanCopyProperty(SerializedProperty source, SerializedProperty target)
        {
            return source.propertyType == target.propertyType &&
                   source.propertyPath == target.propertyPath;
        }

        /// <summary>
        /// Copy property value from source to target based on property type.
        /// </summary>
        private static void CopyPropertyValue(SerializedProperty source, SerializedProperty target)
        {
            switch (source.propertyType)
            {
                case SerializedPropertyType.Integer:
                    target.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    target.boolValue = source.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    target.floatValue = source.floatValue;
                    break;
                case SerializedPropertyType.String:
                    target.stringValue = source.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    target.colorValue = source.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    target.objectReferenceValue = source.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    target.intValue = source.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    target.enumValueIndex = source.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    target.vector2Value = source.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    target.vector3Value = source.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    target.vector4Value = source.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    target.rectValue = source.rectValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    target.animationCurveValue = source.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    target.boundsValue = source.boundsValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    target.quaternionValue = source.quaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    target.exposedReferenceValue = source.exposedReferenceValue;
                    break;
                case SerializedPropertyType.Vector2Int:
                    target.vector2IntValue = source.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    target.vector3IntValue = source.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    target.rectIntValue = source.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    target.boundsIntValue = source.boundsIntValue;
                    break;
            }
        }
    }
}
