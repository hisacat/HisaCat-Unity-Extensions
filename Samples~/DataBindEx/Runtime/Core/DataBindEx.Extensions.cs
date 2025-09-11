using Slash.Unity.DataBind.Core.Presentation;

namespace HisaCat.HUE.DataBindEx.Extensions
{
    public static class ContextTypeExtensions
    {
        public static string GetContextTypeString(System.Type type)
            => type.FullName + ", " + type.Assembly.GetName().FullName;
    }

#if UNITY_EDITOR
    public static class ContextHolderExtensions
    {
        public static void SetContextType(this ContextHolder contextHolder, System.Type type)
        {
            var so = new UnityEditor.SerializedObject(contextHolder);
            var prop = so.FindProperty("contextType");
            prop.stringValue = ContextTypeExtensions.GetContextTypeString(type);
            so.ApplyModifiedProperties();
        }
        public static void SetContextType<TContext>(this ContextHolder contextHolder)
            => SetContextType(contextHolder, typeof(TContext));
    }
#endif
    // public static class DataBindingOperatorExtensions
    // {
    // }
}
