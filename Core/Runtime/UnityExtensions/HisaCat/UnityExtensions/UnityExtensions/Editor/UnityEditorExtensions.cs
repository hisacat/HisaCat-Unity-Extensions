#if UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.UnityExtensions.Editors
{
    public static class EditorExtensions
    {
        [MenuItem("HisaCat/Utils/PlayerPrefs/Delete All")]
        public static void DeleteAllPlayerPrefsKey() => PlayerPrefs.DeleteAll();
    }

    public static class InspectorGUIExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawScriptField(this Editor editor)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var script = editor.serializedObject.FindProperty("m_Script");
                if (script == null)
                {
                    Debug.LogError($"[{nameof(DrawScriptField)}] Failed to find \"m_Script\" property.");
                    return;
                }

                EditorGUILayout.PropertyField(script, true);
            }
        }
    }

    public static class EditorGUILayoutExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadOnlyTextField(string label, string text, GUIStyle style = null, float expandLabelWidth = 0)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (string.IsNullOrEmpty(label) == false)
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 1 + expandLabelWidth));
                }

                if (style == null) style = EditorStyles.label; //EditorStyles.textField
                EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public static class AssetDatabaseExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Object LoadAssetFromGUID(string guid, System.Type type)
            => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), type);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadAssetFromGUID<T>(string guid) where T : Object
            => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));

        [MenuItem("Assets/HisaCat/Copy Asset Type")]
        public static void CopyAssetType()
        {
            var target = Selection.activeObject;
            if (target == null)
            {
                Debug.LogError($"[{nameof(CopyAssetType)}]: Selection is empty!");
                return;
            }
            var type = target.GetType();
            EditorGUIUtility.systemCopyBuffer = type.FullName;
            Debug.Log($"[{nameof(CopyAssetType)}]: Copied \"{target.name}\"s type \"{type.FullName}\" to clipboard.");
        }

        [MenuItem("Assets/HisaCat/Copy Asset GUID")]
        public static void CopyAssetGUID()
        {
            var target = Selection.activeObject;
            if (target == null)
            {
                Debug.LogError($"[{nameof(CopyAssetType)}]: Selection is empty!");
                return;
            }
            var assetPath = AssetDatabase.GetAssetPath(target);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log($"[{nameof(CopyAssetType)}]: Copied \"{target.name}\"s GUID \"{guid}\" to clipboard.");
        }
    }

    public static class SerializePropertyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsArrayElement(this SerializedProperty property)
        {
            var pathParts = property.propertyPath.Split('.');
            return pathParts.Length > 1 && pathParts[^2] == "Array";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetEnumValue<T>(this SerializedProperty enumArrayProperty) where T : System.Enum
        {
            return (T)System.Enum.ToObject(typeof(T), enumArrayProperty.intValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEnumValue<T>(this SerializedProperty enumArrayProperty, T value) where T : System.Enum
        {
            enumArrayProperty.intValue = (int)System.Enum.Parse(typeof(T), value.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetArrayEnumElements<T>(this SerializedProperty enumArrayProperty) where T : System.Enum
        {
            var array = new T[enumArrayProperty.arraySize];
            for (int i = 0; i < enumArrayProperty.arraySize; i++)
            {
                var element = enumArrayProperty.GetArrayElementAtIndex(i);
                array[i] = element.GetEnumValue<T>();
            }
            return array;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetArrayEnumElements<T>(this SerializedProperty enumArrayProperty, IEnumerable<T> values) where T : System.Enum
        {
            enumArrayProperty.ClearArray();
            foreach (var value in values)
                enumArrayProperty.AddArrayEnumElement(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddArrayEnumElement<T>(this SerializedProperty enumArrayProperty, T value) where T : System.Enum
        {
            enumArrayProperty.arraySize++;
            var element = enumArrayProperty.GetArrayElementAtIndex(enumArrayProperty.arraySize - 1);
            element.SetEnumValue(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertArrayEnumElement<T>(this SerializedProperty enumArrayProperty, int index, T value) where T : System.Enum
        {
            enumArrayProperty.InsertArrayElementAtIndex(index);
            var element = enumArrayProperty.GetArrayElementAtIndex(index);
            element.SetEnumValue(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SerializedProperty[] GetArrayElements(this SerializedProperty arrayProperty)
        {
            var elements = new SerializedProperty[arrayProperty.arraySize];
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var element = arrayProperty.GetArrayElementAtIndex(i);
                elements[i] = element;
            }
            return elements;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] GetIntArrayElements(this SerializedProperty intArrayProperty)
        {
            var array = new int[intArrayProperty.arraySize];
            for (int i = 0; i < intArrayProperty.arraySize; i++)
            {
                var element = intArrayProperty.GetArrayElementAtIndex(i);
                array[i] = element.intValue;
            }
            return array;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIntArrayElements(this SerializedProperty intArrayProperty, IEnumerable<int> values)
        {
            intArrayProperty.ClearArray();
            foreach (var value in values)
            {
                intArrayProperty.arraySize++;
                var element = intArrayProperty.GetArrayElementAtIndex(intArrayProperty.arraySize - 1);
                element.intValue = value;
            }
        }
    }
}
#endif
