namespace HisaCat.HUE.DataBindEx.Components.Presentation.Editors
{
    using HisaCat.HUE.DataBindEx.Editors;
    using HisaCat.UnityExtensions.Editors;
    using Slash.Unity.DataBind.Core.Data;
    using Slash.Unity.DataBind.Core.Presentation;
    using Slash.Unity.DataBind.Core.Utils;
    using Slash.Unity.DataBind.Editor.Editors;
    using Slash.Unity.DataBind.Editor.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(MasterType))]
    public class MasterTypeEditor : Editor
    {
        private SerializedProperty contextType = null;
        private SerializedProperty createContext = null;

        private void OnEnable()
        {
            this.contextType = this.serializedObject.FindProperty("contextType");
            this.createContext = this.serializedObject.FindProperty("createContext");
        }

        public override void OnInspectorGUI()
        {
            #region ContextHolderEditor codes (for draw object data)
            if (Application.isPlaying)
            {
                var contextHolder = this.target as ContextHolder;
                if (contextHolder != null)
                {
                    var context = contextHolder.Context;

                    // Use data class if a wrapper is used.
                    if (context is NotifyPropertyChangedDataContext notifyPropertyChangedDataContext)
                    {
                        context = notifyPropertyChangedDataContext.DataObject;
                    }

                    if (context != null)
                    {
                        var contextType = context.GetType().ToString();

                        EditorGUILayout.LabelField("Context", contextType);

                        // Reflect data in context.
                        this.DrawObjectData(context);

                        EditorUtility.SetDirty(contextHolder);
                    }
                }
            }
            #endregion ContextHolderEditor codes (for draw object data)

            var castingContextType = string.IsNullOrEmpty(contextType.stringValue) ? null : ReflectionUtils.FindType(contextType.stringValue);
            var parentContextHolder = GetParentContextHolder();
            var parentContextType = parentContextHolder == null ? null : parentContextHolder.ContextType;
            var parentContextTypePath = parentContextHolder == null ? null : ContextTypeDrawerEx.FormatContextTypePath(parentContextType.FullName);

            EditorGUILayoutExtensions.ReadOnlyTextField("Parent Type", parentContextTypePath, expandLabelWidth: 3);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                EditorGUILayout.PropertyField(this.contextType, new GUIContent("Target Type"));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            if (Application.isPlaying == false)
            {
                if (castingContextType == null)
                {
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Target type not set");
                }
                else if (parentContextType == null)
                {
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Parent type not set");
                }
                else
                {
                    var parentToCurrentCastingable = castingContextType.IsAssignableFrom(parentContextType);
                    var currentToParentCastingable = parentContextType.IsAssignableFrom(castingContextType);
                    if (parentToCurrentCastingable == false && currentToParentCastingable == false)
                    {
                        EditorGUILayoutExtensions.ReadOnlyTextField("Status", "❌ Incompatible");
                    }
                    else
                    {
                        if (parentToCurrentCastingable)
                            EditorGUILayoutExtensions.ReadOnlyTextField("Status", "✔️ Compatible");
                        else
                            EditorGUILayoutExtensions.ReadOnlyTextField("Status", "⚠️ Unknown until runtime");
                    }
                }
            }
            else
            {
                var runtimeParentContext = parentContextHolder == null ? null : parentContextHolder.Context;
                var runtimeParentContextType = runtimeParentContext == null ? null : runtimeParentContext.GetType();
                var runtimeParentContextTypePath = runtimeParentContextType == null ? "null" : ContextTypeDrawerEx.FormatContextTypePath(runtimeParentContextType.FullName);
                EditorGUILayoutExtensions.ReadOnlyTextField("Runtime Parent Type", runtimeParentContextTypePath, expandLabelWidth: 3);


                var runtimeParentContextCastingable = castingContextType.IsAssignableFrom(runtimeParentContextType);
                if (runtimeParentContextCastingable)
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "✔️ Compatible");
                else
                    EditorGUILayoutExtensions.ReadOnlyTextField("Status", "❌ Incompatible");
            }

            // createContext must be false always.
            if (this.createContext.boolValue) this.createContext.boolValue = false;

            this.serializedObject.ApplyModifiedProperties();
        }

        private ContextHolder GetParentContextHolder()
        {
            var component = this.target as Component;
            if (component == null) return null;
            var transform = component.transform;

            var parentContextHolder = (transform.parent == null)
                ? null : transform.parent.GetComponentInParent<ContextHolder>(includeInactive: true);

            return parentContextHolder;
        }

        #region ContextHolderEditor codes (for draw object data)
        private const int MaxLevel = 5;
        private readonly Dictionary<string, bool> foldoutDictionary = new();
        private readonly Dictionary<DataDictionary, object> newKeys = new();
        private Rect invokeEventPopupRect;

        private void DrawObjectData(object obj)
        {
            if (obj == null)
            {
                return;
            }

            this.DrawObjectData(obj, 1);
        }

        private void DrawObjectData(object obj, int level)
        {
            if (obj == null)
            {
                return;
            }

            var prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = level;

            var memberInfos = ContextTypeCache.GetMemberInfos(obj.GetType());
            foreach (var memberInfo in memberInfos)
            {
                if (memberInfo.Property != null)
                {
                    #region Legacy code - it hides private setter fields
                    // var propertySetMethod = memberInfo.Property.GetSetMethod();
                    // if (memberInfo.Property.CanWrite && propertySetMethod != null && propertySetMethod.IsPublic)
                    // {
                    //     var memberValue = memberInfo.Property.GetValue(obj, null);
                    //     var newMemberValue = this.DrawMemberData(memberInfo.Name,
                    //         memberInfo.Property.PropertyType, memberValue, level);
                    //     if (!Equals(newMemberValue, memberValue))
                    //     {
                    //         memberInfo.Property.SetValue(obj, newMemberValue, null);
                    //     }
                    // }
                    #endregion Legacy code - it hides private setter fields

                    #region New code - Shows private setter fields
                    var propertySetMethod = memberInfo.Property.GetSetMethod();
                    var hasPublicSetter = memberInfo.Property.CanWrite && propertySetMethod != null && propertySetMethod.IsPublic;

                    var memberValue = memberInfo.Property.GetValue(obj, null);
                    if (hasPublicSetter)
                    {
                        var newMemberValue = this.DrawMemberData(memberInfo.Name, memberInfo.Property.PropertyType, memberValue, level);
                        if (Equals(newMemberValue, memberValue) == false)
                            memberInfo.Property.SetValue(obj, newMemberValue, null);
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            this.DrawMemberData(memberInfo.Name, memberInfo.Property.PropertyType, memberValue, level);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    #endregion New code - Shows private setter fields
                }
                else if (memberInfo.Method != null)
                {
                    this.DrawObjectMethod(obj, memberInfo.Method);
                }
                else if (memberInfo.Field != null)
                {
                    if (memberInfo.Field.IsPublic)
                    {
                        var memberValue = memberInfo.Field.GetValue(obj);
                        var newMemberValue = this.DrawMemberData(memberInfo.Name,
                            memberInfo.Field.FieldType, memberValue, level);
                        if (!Equals(newMemberValue, memberValue))
                        {
                            memberInfo.Field.SetValue(obj, newMemberValue);
                        }
                    }
                }
            }

            EditorGUI.indentLevel = prevIndentLevel;
        }

        private object DrawMemberData(string memberName, Type memberType, object memberValue, int level)
        {
            if (level < MaxLevel)
            {
                var context = memberValue as Context;
                if (context != null)
                {
                    bool foldout;
                    this.foldoutDictionary.TryGetValue(level + memberName, out foldout);
                    foldout = this.foldoutDictionary[level + memberName] = EditorGUILayout.Foldout(foldout, memberName);
                    if (foldout)
                    {
                        this.DrawObjectData(context, level + 1);
                    }

                    return context;
                }

                var dictionary = memberValue as DataDictionary;
                if (dictionary != null)
                {
                    bool foldout;
                    this.foldoutDictionary.TryGetValue(level + memberName, out foldout);
                    foldout = this.foldoutDictionary[level + memberName] = EditorGUILayout.Foldout(foldout, memberName);
                    if (foldout)
                    {
                        this.DrawDictionaryData(dictionary, level + 1);
                    }

                    return dictionary;
                }

                var enumerable = memberValue as IEnumerable;
                if (enumerable != null && memberType.IsGenericType)
                {
                    var itemType = ReflectionUtils.GetEnumerableItemType(memberType);

                    bool foldout;
                    this.foldoutDictionary.TryGetValue(level + memberName, out foldout);
                    foldout = this.foldoutDictionary[level + memberName] = EditorGUILayout.Foldout(foldout, memberName);
                    if (foldout)
                    {
                        this.DrawEnumerableData(enumerable, memberType, itemType, level + 1);
                    }

                    return enumerable;
                }
            }

            // Draw data trigger.
            var dataTrigger = memberValue as DataTrigger;
            if (dataTrigger != null)
            {
                InspectorUtils.DrawDataTrigger(memberName, dataTrigger);
                return dataTrigger;
            }

            return InspectorUtils.DrawValueField(memberName, memberType, memberValue,
                (name, type, value) => this.DrawCustomTypeData(name, type, value, level));
        }

        private void DrawEnumerableData(IEnumerable enumerable, Type enumerableType, Type itemType, int level)
        {
            var isWriteable = enumerableType.GetInterfaces()
                .Any(x => x.IsGenericType &&
                          x.GetGenericTypeDefinition() == typeof(ICollection<>));

            var prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = level;

            var index = 0;
            List<object> itemsToRemove = null;
            foreach (var item in enumerable)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Item " + index);

                if (isWriteable && GUILayout.Button("Remove"))
                {
                    if (itemsToRemove == null)
                    {
                        itemsToRemove = new List<object>();
                    }

                    itemsToRemove.Add(item);
                }

                EditorGUILayout.EndHorizontal();
                this.DrawMemberData("Item " + index, itemType, item, level);
                ++index;
            }

            if (itemsToRemove != null)
            {
                var removeMethod = enumerableType.GetMethods()
                    .FirstOrDefault(m => m.Name == "Remove" && m.GetParameters().Length == 1);
                if (removeMethod != null)
                {
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        removeMethod.Invoke(enumerable, new[] { itemToRemove });
                    }
                }
            }

            if (isWriteable && GUILayout.Button("New Item"))
            {
                var addMethod = enumerableType.GetMethods()
                    .FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
                if (addMethod != null)
                {
                    var newItem = Activator.CreateInstance(itemType);
                    addMethod.Invoke(enumerable, new[] { newItem });
                }
            }

            EditorGUI.indentLevel = prevIndentLevel;
        }

        private void DrawDictionaryData(DataDictionary dataDictionary, int level)
        {
            var prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = level;

            Dictionary<object, object> changedValues = null;
            foreach (var key in dataDictionary.Keys)
            {
                var value = dataDictionary[key];
                var newValue = this.DrawMemberData("Item " + key, dataDictionary.ValueType, value, level);
                if (!Equals(value, newValue))
                {
                    if (changedValues == null)
                    {
                        changedValues = new Dictionary<object, object>();
                    }

                    changedValues[key] = newValue;
                }
            }

            if (changedValues != null)
            {
                foreach (var key in changedValues.Keys)
                {
                    dataDictionary[key] = changedValues[key];
                }
            }

            GUILayout.BeginHorizontal();

            object newKey;
            this.newKeys.TryGetValue(dataDictionary, out newKey);
            var keyType = dataDictionary.KeyType;
            const string NewKeyLabel = "New:";
            if (keyType == typeof(string))
            {
                this.newKeys[dataDictionary] = newKey = EditorGUILayout.TextField(NewKeyLabel, (string)newKey);
            }
            else if (TypeInfoUtils.IsEnum(keyType))
            {
                if (newKey == null)
                {
                    newKey = Enum.GetValues(keyType).GetValue(0);
                }

                this.newKeys[dataDictionary] = newKey = EditorGUILayout.EnumPopup(NewKeyLabel, (Enum)newKey);
            }

            if (GUILayout.Button("+") && newKey != null)
            {
                var valueType = dataDictionary.ValueType;
                dataDictionary.Add(newKey, valueType.IsValueType ? Activator.CreateInstance(valueType) : null);
            }

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel = prevIndentLevel;
        }

        private void DrawObjectMethod(object context, MethodInfo method)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(method.Name));

            if (GUILayout.Button("Invoke"))
            {
                var parameterInfos = method.GetParameters();
                if (parameterInfos.Length > 0)
                {
                    var invokeEventPopup = new InvokeEventPopup(parameterInfos,
                        parameters => method.Invoke(context, parameters));
                    PopupWindow.Show(this.invokeEventPopupRect, invokeEventPopup);
                }
                else
                {
                    method.Invoke(context, null);
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                // Cover button with popup.
                var buttonRect = GUILayoutUtility.GetLastRect();
                this.invokeEventPopupRect = buttonRect;
                this.invokeEventPopupRect.position = new Vector2(buttonRect.position.x,
                    buttonRect.position.y - buttonRect.size.y);
            }

            EditorGUILayout.EndHorizontal();
        }

        private object DrawCustomTypeData(string memberName, Type memberType, object memberValue, int level)
        {
            if (memberValue == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(memberName, "null");
                var canCreateInstance = memberType.GetConstructor(Type.EmptyTypes) != null;
                if (canCreateInstance && GUILayout.Button("Create"))
                {
                    memberValue = Activator.CreateInstance(memberType);
                }
                EditorGUILayout.EndHorizontal();

                return memberValue;
            }

            bool foldout;
            this.foldoutDictionary.TryGetValue(level + memberName, out foldout);
            foldout = this.foldoutDictionary[level + memberName] = EditorGUILayout.Foldout(foldout, memberName);
            if (foldout)
            {
                this.DrawObjectData(memberValue, level + 1);
            }

            return memberValue;
        }
        #endregion ContextHolderEditor codes (for draw object data)
    }
}
