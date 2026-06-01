#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HisaCat.UGUI
{
    /// <summary>
    /// Custom inspector for <see cref="AspectRatioLayoutElement"/>.
    /// </summary>
    [CustomEditor(typeof(AspectRatioLayoutElement))]
    [CanEditMultipleObjects]
    public class AspectRatioLayoutElementEditor : Editor
    {
        private SerializedProperty m_AspectMode;
        private SerializedProperty m_AspectRatio;
        private SerializedProperty m_LayoutPriority;

        /// <summary>
        /// Caches serialized properties.
        /// </summary>
        private void OnEnable()
        {
            this.m_AspectMode = this.serializedObject.FindProperty(nameof(this.m_AspectMode));
            this.m_AspectRatio = this.serializedObject.FindProperty(nameof(this.m_AspectRatio));
            this.m_LayoutPriority = this.serializedObject.FindProperty(nameof(this.m_LayoutPriority));
        }

        /// <summary>
        /// Draws the inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            DrawProperties();

            this.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            DrawCalculatedPreferredSize();
            DrawValidationHelpBoxes();
        }

        /// <summary>
        /// Draws editable serialized properties.
        /// </summary>
        private void DrawProperties()
        {
            EditorGUILayout.PropertyField(
                this.m_AspectMode,
                new GUIContent(
                    "Aspect Mode",
                    "Determines which preferred size is calculated from the opposite axis."));

            EditorGUILayout.PropertyField(
                this.m_AspectRatio,
                new GUIContent(
                    "Aspect Ratio",
                    "Width divided by height. For example, 16:9 is 16 / 9."));

            EditorGUILayout.PropertyField(
                this.m_LayoutPriority,
                new GUIContent(
                    "Layout Priority",
                    "Priority used when multiple ILayoutElement components exist on the same object."));
        }

        /// <summary>
        /// Draws read-only preferred size values calculated by selected components.
        /// </summary>
        private void DrawCalculatedPreferredSize()
        {
            var elements = targets
                .OfType<AspectRatioLayoutElement>()
                .Where(e => e != null)
                .ToArray();

            var widthElements = elements
                .Where(e => e.aspectMode == AspectRatioLayoutElement.AspectMode.HeightControlsWidth)
                .ToArray();

            var heightElements = elements
                .Where(e => e.aspectMode == AspectRatioLayoutElement.AspectMode.WidthControlsHeight)
                .ToArray();

            EditorGUILayout.LabelField("Calculated Preferred Size", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                if (widthElements.Length == 0 && heightElements.Length == 0)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField("Preferred Size", "Not Provided");
                    }

                    return;
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    if (widthElements.Length > 0)
                    {
                        DrawReadonlyFloatField(
                            "Preferred Width",
                            widthElements.Select(e => e.preferredWidth));
                    }

                    if (heightElements.Length > 0)
                    {
                        DrawReadonlyFloatField(
                            "Preferred Height",
                            heightElements.Select(e => e.preferredHeight));
                    }
                }
            }
        }

        /// <summary>
        /// Draws a disabled float field that supports mixed values.
        /// </summary>
        /// <param name="label">Field label.</param>
        /// <param name="values">Values from selected components.</param>
        private static void DrawReadonlyFloatField(string label, IEnumerable<float> values)
        {
            var valueArray = values.ToArray();

            if (valueArray.Length == 0)
                return;

            var previousMixedValue = EditorGUI.showMixedValue;

            EditorGUI.showMixedValue = HasMixedFloatValues(valueArray);
            EditorGUILayout.FloatField(label, valueArray[0]);

            EditorGUI.showMixedValue = previousMixedValue;
        }

        /// <summary>
        /// Determines whether the given float values should be displayed as mixed.
        /// </summary>
        /// <param name="values">Float values to compare.</param>
        /// <returns><c>true</c> when values are different; otherwise, <c>false</c>.</returns>
        private static bool HasMixedFloatValues(float[] values)
        {
            if (values.Length <= 1)
                return false;

            var firstValue = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                if (!Mathf.Approximately(firstValue, values[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Draws warnings for common invalid layout configurations.
        /// </summary>
        private void DrawValidationHelpBoxes()
        {
            var elements = targets
                .OfType<AspectRatioLayoutElement>()
                .Where(e => e != null)
                .ToArray();

            if (elements.Length == 0)
                return;

            var contexts = elements
                .Select(LayoutValidationContext.Create)
                .ToArray();

            DrawIgnoredLayoutWarning(contexts);
            DrawMissingParentLayoutGroupWarning(contexts);
            DrawDisabledParentLayoutGroupWarning(contexts);
            DrawMissingControlChildSizeWarning(contexts);
        }

        /// <summary>
        /// Draws a warning when selected elements are ignored by layout.
        /// </summary>
        /// <param name="contexts">Validation contexts.</param>
        private static void DrawIgnoredLayoutWarning(LayoutValidationContext[] contexts)
        {
            var ignoredCount = contexts.Count(e => e.isIgnoredByLayout);

            if (ignoredCount <= 0)
                return;

            EditorGUILayout.HelpBox(
                BuildCountMessage(
                    ignoredCount,
                    "Selected object is ignored by layout, so AspectRatioLayoutElement will not affect its parent layout.",
                    "Some selected objects are ignored by layout, so AspectRatioLayoutElement will not affect their parent layouts."),
                MessageType.Warning);
        }

        /// <summary>
        /// Draws a warning when there is no supported parent layout group.
        /// </summary>
        /// <param name="contexts">Validation contexts.</param>
        private static void DrawMissingParentLayoutGroupWarning(LayoutValidationContext[] contexts)
        {
            var invalidContexts = contexts
                .Where(e => !e.isIgnoredByLayout)
                .Where(e => e.parentLayoutGroup == null)
                .ToArray();

            if (invalidContexts.Length <= 0)
                return;

            EditorGUILayout.HelpBox(
                BuildCountMessage(
                    invalidContexts.Length,
                    "Parent does not have a HorizontalLayoutGroup or VerticalLayoutGroup. This component should be used as a child of a supported layout group.",
                    "Some selected objects do not have a HorizontalLayoutGroup or VerticalLayoutGroup parent. This component should be used as a child of a supported layout group."),
                MessageType.Warning);
        }

        /// <summary>
        /// Draws a warning when the parent layout group exists but is disabled.
        /// </summary>
        /// <param name="contexts">Validation contexts.</param>
        private static void DrawDisabledParentLayoutGroupWarning(LayoutValidationContext[] contexts)
        {
            var disabledGroups = contexts
                .Where(e => !e.isIgnoredByLayout)
                .Where(e => e.parentLayoutGroup != null)
                .Where(e => !e.parentLayoutGroup.isActiveAndEnabled)
                .Select(e => e.parentLayoutGroup)
                .Distinct()
                .ToArray();

            if (disabledGroups.Length <= 0)
                return;

            EditorGUILayout.HelpBox(
                $"Parent LayoutGroup is disabled or inactive: {FormatObjectNames(disabledGroups)}",
                MessageType.Warning);
        }

        /// <summary>
        /// Draws warnings when the parent layout group does not control the required child size axis.
        /// </summary>
        /// <param name="contexts">Validation contexts.</param>
        private static void DrawMissingControlChildSizeWarning(LayoutValidationContext[] contexts)
        {
            var missingWidthGroups = contexts
                .Where(e => !e.isIgnoredByLayout)
                .Where(e => e.aspectMode == AspectRatioLayoutElement.AspectMode.HeightControlsWidth)
                .Where(e => e.parentLayoutGroup != null)
                .Where(e => e.parentLayoutGroup.isActiveAndEnabled)
                .Where(e => !e.parentLayoutGroup.childControlWidth)
                .Select(e => e.parentLayoutGroup)
                .Distinct()
                .ToArray();

            if (missingWidthGroups.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "Aspect Mode 'Height Controls Width' requires the parent LayoutGroup to enable " +
                    $"'Control Child Size - Width': {FormatObjectNames(missingWidthGroups)}",
                    MessageType.Warning);
            }

            var missingHeightGroups = contexts
                .Where(e => !e.isIgnoredByLayout)
                .Where(e => e.aspectMode == AspectRatioLayoutElement.AspectMode.WidthControlsHeight)
                .Where(e => e.parentLayoutGroup != null)
                .Where(e => e.parentLayoutGroup.isActiveAndEnabled)
                .Where(e => !e.parentLayoutGroup.childControlHeight)
                .Select(e => e.parentLayoutGroup)
                .Distinct()
                .ToArray();

            if (missingHeightGroups.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "Aspect Mode 'Width Controls Height' requires the parent LayoutGroup to enable " +
                    $"'Control Child Size - Height': {FormatObjectNames(missingHeightGroups)}",
                    MessageType.Warning);
            }
        }

        /// <summary>
        /// Builds a singular or plural message depending on count.
        /// </summary>
        /// <param name="count">Number of affected objects.</param>
        /// <param name="singular">Message used when count is one.</param>
        /// <param name="plural">Message used when count is greater than one.</param>
        /// <returns>The selected message.</returns>
        private static string BuildCountMessage(int count, string singular, string plural)
        {
            return count == 1 ? singular : plural;
        }

        /// <summary>
        /// Formats Unity object names for warning messages.
        /// </summary>
        /// <param name="objects">Objects to format.</param>
        /// <returns>Comma-separated object names.</returns>
        private static string FormatObjectNames(Object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return "None";

            return string.Join(", ", objects.Select(e => $"'{e.name}'"));
        }

        /// <summary>
        /// Stores layout validation data for a selected <see cref="AspectRatioLayoutElement"/>.
        /// </summary>
        private readonly struct LayoutValidationContext
        {
            /// <summary>
            /// Selected aspect ratio layout element.
            /// </summary>
            public readonly AspectRatioLayoutElement element;

            /// <summary>
            /// Aspect mode of the selected element.
            /// </summary>
            public readonly AspectRatioLayoutElement.AspectMode aspectMode;

            /// <summary>
            /// Whether this child is ignored by the parent layout group.
            /// </summary>
            public readonly bool isIgnoredByLayout;

            /// <summary>
            /// Parent horizontal or vertical layout group.
            /// </summary>
            public readonly HorizontalOrVerticalLayoutGroup parentLayoutGroup;

            /// <summary>
            /// Initializes a new instance of the <see cref="LayoutValidationContext"/> struct.
            /// </summary>
            /// <param name="element">Selected aspect ratio layout element.</param>
            /// <param name="aspectMode">Aspect mode of the selected element.</param>
            /// <param name="isIgnoredByLayout">Whether this child is ignored by layout.</param>
            /// <param name="parentLayoutGroup">Parent horizontal or vertical layout group.</param>
            private LayoutValidationContext(
                AspectRatioLayoutElement element,
                AspectRatioLayoutElement.AspectMode aspectMode,
                bool isIgnoredByLayout,
                HorizontalOrVerticalLayoutGroup parentLayoutGroup)
            {
                this.element = element;
                this.aspectMode = aspectMode;
                this.isIgnoredByLayout = isIgnoredByLayout;
                this.parentLayoutGroup = parentLayoutGroup;
            }

            /// <summary>
            /// Creates validation context from an <see cref="AspectRatioLayoutElement"/>.
            /// </summary>
            /// <param name="element">Element to validate.</param>
            /// <returns>Created validation context.</returns>
            public static LayoutValidationContext Create(AspectRatioLayoutElement element)
            {
                var isIgnoredByLayout = IsIgnoredByLayout(element);

                var parentTransform = element.transform.parent as RectTransform;
                var parentLayoutGroup = parentTransform != null
                    ? parentTransform.GetComponent<HorizontalOrVerticalLayoutGroup>()
                    : null;

                return new LayoutValidationContext(
                    element,
                    element.aspectMode,
                    isIgnoredByLayout,
                    parentLayoutGroup);
            }

            /// <summary>
            /// Determines whether the given element is ignored by its parent layout group.
            /// </summary>
            /// <param name="element">Element to check.</param>
            /// <returns><c>true</c> if ignored by layout; otherwise, <c>false</c>.</returns>
            private static bool IsIgnoredByLayout(AspectRatioLayoutElement element)
            {
                // LayoutGroup ignores a child when any ILayoutIgnorer on the child
                // reports ignoreLayout == true.
                var ignorers = element.GetComponents<MonoBehaviour>()
                    .OfType<ILayoutIgnorer>();

                return ignorers.Any(e => e.ignoreLayout);
            }
        }
    }
}
#endif
