using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HisaCat.UI
{
    public class MultiGraphicButton : Button
    {
        [SerializeField] private bool m_ExceptUnderSelectables = false;
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (this.transition == Transition.ColorTint)
            {
                var targetColor =
                    state == SelectionState.Disabled ? colors.disabledColor :
                    state == SelectionState.Highlighted ? colors.highlightedColor :
                    state == SelectionState.Normal ? colors.normalColor :
                    state == SelectionState.Pressed ? colors.pressedColor :
                    state == SelectionState.Selected ? colors.selectedColor : Color.white;

                if (m_ExceptUnderSelectables)
                {
                    var unserSelectables = GetComponentsInChildren<Selectable>().Where(x => x.gameObject != this.gameObject);
                    var exceptGraphics = unserSelectables.SelectMany(x => x.GetComponentsInChildren<Graphic>());
                    var graphics = GetComponentsInChildren<Graphic>().Except(exceptGraphics);
                    using (var enumerator = graphics.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var graphic = enumerator.Current;
                            graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
                        }
                    }
                }
                else
                {
                    var graphics = GetComponentsInChildren<Graphic>();
                    int count = graphics.Length;
                    for (int i = 0; i < count; i++)
                    {
                        graphics[i].CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
                    }
                }
            }
            else
            {
                base.DoStateTransition(state, instant);
            }
        }
    }
}

#if UNITY_EDITOR
namespace UnityEditor.UI
{
    [CustomEditor(typeof(HisaCat.UI.MultiGraphicButton), true)]
    [CanEditMultipleObjects]
    public class MultiGraphicButtonEditor : ButtonEditor
    {
        SerializedProperty m_ExceptUnderSelectablesProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ExceptUnderSelectablesProperty = serializedObject.FindProperty("m_ExceptUnderSelectables");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ExceptUnderSelectablesProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
