using Slash.Unity.DataBind.Core.Presentation;
using Slash.Unity.DataBind.Core.Utils;
using Slash.Unity.DataBind.Editor.Utils;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HisaCat.HUE.DataBindEx.Extensions.Editors
{
    public class DataBindEditorExtension : Editor
    {
        public static DataBindingOperator[] GetDataBindComponents()
        {
            return StageUtility.GetCurrentStageHandle().FindComponentsOfType<DataBindingOperator>();
            //Command
        }

        [MenuItem("HisaCat/DataBind/Find missing context type")]
        public static void FindMissingContextType()
        {
            var components = GetDataBindComponents();
            //DataBinding
            //string Path (ContextPath)
            int count = components.Length;
            for (int i = 0; i < count; i++)
            {
                var component = components[i];

                var type = component.GetType();
                System.Reflection.FieldInfo[] fieldInfo = type.GetFields();
                foreach (System.Reflection.FieldInfo info in fieldInfo)
                {
                    //Debug.Log("Field " + info.Name, component.gameObject);
                    if (info.FieldType == typeof(DataBinding))
                    {
                        var value = info.GetValue(component) as DataBinding;
                        if (value != null)
                        {
                            if (value.Type == DataBindingType.Context)
                            {
                                var contextPath = value.Path; //Context Path
                                if (string.IsNullOrEmpty(contextPath) == false)
                                {
                                    //See PathPropertyDrawer

                                    var pathAttributeContainsField = typeof(DataBinding).GetFields()
                                        .FirstOrDefault(x => x.CustomAttributes
                                        .Any(y => y.AttributeType == typeof(ContextPathAttribute)));
                                    var contextPathAttribute = pathAttributeContainsField.GetCustomAttributes(typeof(ContextPathAttribute), true).FirstOrDefault() as ContextPathAttribute;

                                    // Check if target object has a custom context type.
                                    //var contextTypeProperty = property.serializedObject.FindProperty("ContextType");
                                    //var dataContextType = contextTypeProperty != null
                                    //    ? ReflectionUtils.FindType(contextTypeProperty.stringValue)
                                    //    : ContextTypeEditorUtils.GetContextType((Component)targetObject);
                                    var dataContextType = ContextTypeEditorUtils.GetContextType(component);

                                    var dataContextPaths = ContextTypeCache.GetPaths(
                                        dataContextType,
                                        contextPathAttribute != null ? contextPathAttribute.Filter : ContextMemberFilter.All);

                                    //Debug.Log(dataContextPaths.Count);

                                    if (dataContextPaths.Contains(contextPath) == false)
                                    {
                                        Debug.Log($"FindDataBindFields: <b>{component.GetType().Name}</b> contains missing context type <b>{contextPath}</b>", component);
                                    }
                                }
                            }
                        }
                    }
                }
                /*
                System.Reflection.PropertyInfo[] propertyInfo = type.GetProperties();
                foreach (System.Reflection.PropertyInfo info in propertyInfo)
                {
                }
                */
            }
            Debug.Log("FindDataBindFields: Done");
        }
    }
}
