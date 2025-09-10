using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace HisaCat.UnityExtensions.Editors
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
        public static void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(label))
                    EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                var style = EditorStyles.label; //EditorStyles.textField
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
}
