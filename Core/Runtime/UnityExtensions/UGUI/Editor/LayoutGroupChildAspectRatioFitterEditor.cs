using System.Linq;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HisaCat.UGUI
{
    [CustomEditor(typeof(LayoutGroupChildAspectRatioFitter))]
    [CanEditMultipleObjects]
    public class LayoutGroupChildAspectRatioFitterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var comp = target as LayoutGroupChildAspectRatioFitter;

            var targetComps = this.targets.Select(e => e as LayoutGroupChildAspectRatioFitter);
            var widthComps = targetComps.Where(e => e.aspectMode == LayoutGroupChildAspectRatioFitter.AspectMode.HeightControlsWidth);
            var heightComps = targetComps.Where(e => e.aspectMode == LayoutGroupChildAspectRatioFitter.AspectMode.WidthControlsHeight);
            var (widthCompsCount, heightCompsCount) = (widthComps.Count(), heightComps.Count());

            #region Draw Calculated Preferred Size
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Calculated Preferred Size");
            {
                EditorGUI.indentLevel++;
                {
                    void DrawReadonlyTextField(string label, string text)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField(label, text);
                        EditorGUI.EndDisabledGroup();
                    }

                    if (widthCompsCount > 0)
                    {
                        EditorGUI.showMixedValue = widthComps.GroupBy(e => e.preferredWidth).Count() > 1;
                        DrawReadonlyTextField("Preferred Width", widthComps.First().preferredWidth.ToString());
                        EditorGUI.showMixedValue = false;
                    }
                    if (heightCompsCount > 0)
                    {
                        EditorGUI.showMixedValue = heightComps.GroupBy(e => e.preferredHeight).Count() > 1;
                        DrawReadonlyTextField("Preferred Height", heightComps.First().preferredHeight.ToString());
                        EditorGUI.showMixedValue = false;
                    }
                }
                EditorGUI.indentLevel--;
            }
            #endregion Draw Calculated Preferred Size

            Behaviour group = null;
            HorizontalLayoutGroup hGroup = null;
            VerticalLayoutGroup vGroup = null;
            {
                var ignorer = comp.GetComponent<ILayoutIgnorer>();
                if (ignorer == null || ignorer.ignoreLayout == false)
                {
                    RectTransform parent = comp.transform.parent as RectTransform;
                    if (parent != null)
                    {
                        group = parent.GetComponent<ILayoutGroup>() as Behaviour;
                        switch (group)
                        {
                            case HorizontalLayoutGroup _group:
                                hGroup = _group;
                                break;
                            case VerticalLayoutGroup _group:
                                vGroup = _group;
                                break;
                        }
                    }
                }
            }

            if (hGroup == null && vGroup == null)
            {
                EditorGUILayout.HelpBox("Parent does not have a horizontal or vertical layout group component. This component should be a child of a horizontal or vertical layout group.", MessageType.Warning);
                return;
            }

            bool controlChildWidth = false;
            bool controlChildHeight = false;
            {
                if (hGroup != null)
                {
                    controlChildWidth = hGroup.childControlWidth;
                    controlChildHeight = hGroup.childControlHeight;
                }
                else if (vGroup != null)
                {
                    controlChildWidth = vGroup.childControlWidth;
                    controlChildHeight = vGroup.childControlHeight;
                }
            }

            if (widthCompsCount > 0 && controlChildWidth == false)
                EditorGUILayout.HelpBox(
                    $"For Aspect Mode '{comp.aspectMode}' to work properly, " +
                    $"please enable 'Control Child Size' - 'Width' option in the parent LayoutGroup '{group.name}'.",
                    MessageType.Warning);
            if (heightCompsCount > 0 && controlChildHeight == false)
                EditorGUILayout.HelpBox(
                    $"For Aspect Mode '{comp.aspectMode}' to work properly, " +
                    $"please enable 'Control Child Size' - 'Height' option in the parent LayoutGroup '{group.name}'.",
                    MessageType.Warning);
        }
    }
}
