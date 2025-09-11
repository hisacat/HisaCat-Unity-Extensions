using Slash.Unity.DataBind.Core.Data;
using Slash.Unity.DataBind.Core.Presentation;

namespace HisaCat.HUE.DataBindEx
{
    public abstract class ContextFixedContextHolder<TType> : ContextHolder where TType : Context
    {
        protected override void Awake()
        {
            base.Awake();
            UpdateContext();
        }
        protected abstract Context GetContext();
        protected void UpdateContext()
        {
            this.Context = GetContext();
        }

        private void Reset()
        {
#if UNITY_EDITOR
            var serializedObject = new UnityEditor.SerializedObject(this);
            var contextType = serializedObject.FindProperty("contextType");
            contextType.stringValue = typeof(TType).AssemblyQualifiedName;
            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
