using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace HisaCat.StateMachineBehaviours
{
    [CustomEditor(typeof(CancelTrigger))]
    public class CancelTriggerEditor : Editor
    {
        private SerializedProperty m_TriggerName = null;
        private SerializedProperty m_CancelOnEnter = null;
        private SerializedProperty m_CancelOnExit = null;
        //private SerializedProperty m_CancelOnUpdate = null;

        private void OnEnable()
        {
            this.m_TriggerName = serializedObject.FindProperty(nameof(this.m_TriggerName));
            this.m_CancelOnEnter = serializedObject.FindProperty(nameof(this.m_CancelOnEnter));
            this.m_CancelOnExit = serializedObject.FindProperty(nameof(this.m_CancelOnExit));
            //this.m_CancelOnUpdate = serializedObject.FindProperty(nameof(this.m_CancelOnUpdate));
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            CancelTrigger behaviour = (CancelTrigger)this.target;

            AnimatorController controller = GetCurrentAnimatorController(behaviour);

            if (controller != null)
            {
                string[] triggerNames = GetTriggerParameterNames(controller);
                int selectedIndex = Mathf.Max(0, System.Array.IndexOf(triggerNames, this.m_TriggerName.stringValue));

                int newIndex = EditorGUILayout.Popup("Trigger Name", selectedIndex, triggerNames);
                this.m_TriggerName.stringValue = triggerNames[newIndex];
            }
            else
            {
                EditorGUILayout.HelpBox("AnimatorController not found. Assign trigger name manually.", MessageType.Warning);
                EditorGUILayout.PropertyField(this.m_TriggerName);
            }

            EditorGUILayout.PropertyField(this.m_CancelOnEnter);
            EditorGUILayout.PropertyField(this.m_CancelOnExit);
            //EditorGUILayout.PropertyField(this.m_CancelOnUpdate);

            serializedObject.ApplyModifiedProperties();
        }

        private AnimatorController GetCurrentAnimatorController(CancelTrigger behaviour)
        {
            var context = AnimatorController.FindStateMachineBehaviourContext(behaviour);
            if (context == null || context.Length <= 0) return null;
            return context[0].animatorController;

            //var path = AssetDatabase.GetAssetPath(behaviour);
            //var animatorController = AssetDatabase.LoadMainAssetAtPath(path) as AnimatorController;
            //return animatorController;
        }

        private string[] GetTriggerParameterNames(AnimatorController controller)
        {
            var parameters = controller.parameters;
            var triggers = new System.Collections.Generic.List<string>();
            foreach (var param in parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    triggers.Add(param.name);
            }

            return triggers.Count > 0 ? triggers.ToArray() : new[] { "(None)" };
        }
    }
}
