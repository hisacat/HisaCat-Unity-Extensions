using HisaCat.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HisaCat.Fonts
{
    [CustomEditor(typeof(I18NFontDataAsset), true)]
    public class I18NFontDataAssetEditor : Editor
    {
        private SerializedProperty m_BaseFont = null;
        private SerializedProperty m_I18NFonts = null;

        private void OnEnable()
        {
            this.m_BaseFont = this.serializedObject.FindProperty(nameof(this.m_BaseFont));
            this.m_I18NFonts = this.serializedObject.FindProperty(nameof(this.m_I18NFonts));
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            var selectedFolder = EditorGUILayout.ObjectField("Auto update from folder", null, typeof(DefaultAsset), false) as DefaultAsset;
            if (selectedFolder != null)
            {
                // Clear all existing fonts.
                this.m_I18NFonts.arraySize = 0;

                // Get all font assets in the selected folder.
                var guids = AssetDatabase.FindAssets($"t:{nameof(TMP_FontAsset)}", new[] { AssetDatabase.GetAssetPath(selectedFolder) });
                var fontAssets = new List<TMP_FontAsset>();
                foreach (var guid in guids)
                {
                    var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (fontAsset != null) fontAssets.Add(fontAsset);
                }

                // Find target language font and add to I18NFonts.
                foreach (var fontAsset in fontAssets)
                {
                    SystemLanguage targetLanguage = SystemLanguage.Unknown;
                    foreach (SystemLanguage lang in Enum.GetValues(typeof(SystemLanguage)))
                    {
                        if (lang == SystemLanguage.Unknown) continue;
                        if (lang == SystemLanguage.Chinese) continue; // It same with ChineseSimplified

                        var prefix = SystemLanguageToLocaleString.ToLocaleString(lang);
                        if (lang == SystemLanguage.English) prefix = "[LatinPlus]";
                        if (fontAsset.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            targetLanguage = lang;
                            break;
                        }
                    }

                    if (targetLanguage == SystemLanguage.Unknown)
                    {
                        Debug.LogWarning($"Cannot find target language for \"{fontAsset.name}\"", fontAsset);
                        continue;
                    }

                    this.m_I18NFonts.arraySize++;
                    var element = this.m_I18NFonts.GetArrayElementAtIndex(this.m_I18NFonts.arraySize - 1);
                    var m_Langauge = element.FindPropertyRelative("m_Language");
                    var m_Font = element.FindPropertyRelative("m_Font");

                    m_Langauge.enumValueIndex = Array.IndexOf(m_Langauge.enumNames, targetLanguage.ToString());
                    m_Font.objectReferenceValue = fontAsset;
                    Debug.Log($"Language \"{targetLanguage} ({targetLanguage.ToLocaleString()})\" added from \"{fontAsset.name}\"", fontAsset);
                }

                // Move english font to top if exists.
                {
                    var count = this.m_I18NFonts.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        var element = this.m_I18NFonts.GetArrayElementAtIndex(i);
                        var m_Langauge = element.FindPropertyRelative("m_Language");
                        if (m_Langauge.enumValueIndex == Array.IndexOf(m_Langauge.enumNames, SystemLanguage.English.ToString()))
                        {
                            this.m_I18NFonts.MoveArrayElement(i, 0);
                            break;
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(this.m_BaseFont);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "런타임에 선택된 언어가 첫번째 Fallback으로 들어가며, 이후 다른 언어가 순차적으로 추가됩니다."
                + "\r\n두번째 Fallback으로는 기본 English(LatinPlus)를 사용하고 싶을 것입니다."
                + "\r\n그러니 English font의 순서를 항상 첫번째로 유지하세요.", MessageType.Info);
            EditorGUILayout.PropertyField(this.m_I18NFonts);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
