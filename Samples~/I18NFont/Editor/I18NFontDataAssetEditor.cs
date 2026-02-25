using HisaCat.HUE.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HisaCat.HUE.Fonts
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
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
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

            var oldBaseFont = this.m_BaseFont.objectReferenceValue as TMP_FontAsset;
            EditorGUILayout.PropertyField(this.m_BaseFont);
            EditorGUILayout.HelpBox(
                $"실제로 사용할 TMP_Font asset입니다. 비어있는 Font를 사용하세요."
                + $"\r\n메뉴: '{I18NBaseFontGenerator.MenuItemName}'", MessageType.Info);
            EditorGUILayout.HelpBox(
                "'Base Font'에 런타임에 선택된 언어가 첫번째 Fallback으로 들어가며,"
                + "\r\n이후 나머지 'I18N Fonts' 배열에 있는 폰트가 순차적으로 추가됩니다."
                + "\r\n두번째 Fallback으로는 기본적으로 English(LatinPlus)를 사용하고 싶을 것입니다."
                + "\r\n그러니 English font의 순서를 되도록 항상 첫번째로 유지하세요.", MessageType.Info);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.PropertyField(this.m_I18NFonts);

            if (serializedObject.ApplyModifiedProperties())
            {
                var curBaseFont = this.m_BaseFont.objectReferenceValue as TMP_FontAsset;

                bool wasAnyFallbackFontChanged = false;
                if (oldBaseFont != null && oldBaseFont != curBaseFont)
                {
                    Undo.RecordObject(oldBaseFont, "Clear Base Font Fallbacks");
                    oldBaseFont.fallbackFontAssetTable.Clear();
                    EditorUtility.SetDirty(oldBaseFont);

                    wasAnyFallbackFontChanged = true;
                }

                if (curBaseFont != null)
                {
                    if (this.target is not I18NFontDataAsset asset)
                    {
                        Debug.LogError($"[{nameof(I18NFontDataAssetEditor)}] Target '{target.name}' is not {nameof(I18NFontDataAsset)}!", target);
                    }
                    else
                    {
                        Undo.RecordObject(curBaseFont, "Update Base Font Fallbacks");
                        I18NFontUtility.UpdateBaseFontFallbacks(asset, curBaseFont, LocalizationManager.SelectedLanguage, forceRefreshAllTmpFonts: false);
                        EditorUtility.SetDirty(curBaseFont);

                        wasAnyFallbackFontChanged = true;
                    }
                }

                if (wasAnyFallbackFontChanged)
                    I18NFontUtility.ForceRefreshAllTMPTextsForFallbackFonts();
            }
        }
    }
}
