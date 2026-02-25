using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HisaCat.HUE.UnityExtensions.Editors;
using System;

namespace HisaCat.HUE.Localization
{
    [CustomEditor(typeof(LocalizationSettingsAsset))]
    public class LocalizationSettingsAssetEditor : Editor
    {
        private SerializedProperty m_LocalizedJsonsPath = null;

        private SerializedProperty m_DefaultLanguage = null;
        private SerializedProperty m_FallbackLanguage = null;
        private SerializedProperty m_SupportLanguages = null;

        private SerializedProperty m_PrintMissingLanguageLogs = null;
        private SerializedProperty m_PrintMissingKeyLogs = null;
        private SerializedProperty m_AutoUpdateLocalizedTextOnEditor = null;
        private void OnEnable()
        {
            this.m_LocalizedJsonsPath = this.serializedObject.FindProperty(nameof(m_LocalizedJsonsPath));

            this.m_DefaultLanguage = this.serializedObject.FindProperty(nameof(m_DefaultLanguage));
            this.m_FallbackLanguage = this.serializedObject.FindProperty(nameof(m_FallbackLanguage));
            this.m_SupportLanguages = this.serializedObject.FindProperty(nameof(m_SupportLanguages));

            this.m_PrintMissingLanguageLogs = this.serializedObject.FindProperty(nameof(m_PrintMissingLanguageLogs));
            this.m_PrintMissingKeyLogs = this.serializedObject.FindProperty(nameof(m_PrintMissingKeyLogs));
            this.m_AutoUpdateLocalizedTextOnEditor = this.serializedObject.FindProperty(nameof(m_AutoUpdateLocalizedTextOnEditor));
        }
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.m_LocalizedJsonsPath);
            EditorGUILayout.HelpBox("The path where the Localized json files are located relative to the 'Resources' folder.", MessageType.Info);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.PropertyField(this.m_DefaultLanguage);
            EditorGUILayout.PropertyField(this.m_FallbackLanguage);
            EditorGUILayout.PropertyField(this.m_SupportLanguages);
            {
                if (GUILayout.Button("Add All Languages"))
                    this.m_SupportLanguages.SetArrayEnumElements(AllLanguagesSet);
                if (GUILayout.Button("Clear All Languages"))
                    this.m_SupportLanguages.ClearArray();

                // Duplicate language checks
                {
                    var hasDuplicateValue = false;
                    HashSet<SystemLanguage> set = new();
                    for (int i = 0; i < this.m_SupportLanguages.arraySize; i++)
                    {
                        var element = this.m_SupportLanguages.GetArrayElementAtIndex(i);
                        if (set.Add(element.GetEnumValue<SystemLanguage>()) == false)
                            hasDuplicateValue = true;
                    }

                    if (hasDuplicateValue)
                    {
                        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
                        EditorGUILayout.HelpBox("Duplicate language values are detected.", MessageType.Warning);
                        if (GUILayout.Button("Remove Duplicate Languages"))
                            this.m_SupportLanguages.SetArrayEnumElements(set);
                    }
                }

                // Default & Fallback langauge
                {
                    var supportLanguages = this.m_SupportLanguages.GetArrayEnumElements<SystemLanguage>();

                    var fallbackLanguage = this.m_FallbackLanguage.GetEnumValue<SystemLanguage>();
                    var defaultLanguage = this.m_DefaultLanguage.GetEnumValue<SystemLanguage>();
                    var necessaryLanguagesSet = new HashSet<SystemLanguage>() { fallbackLanguage, defaultLanguage };
                    foreach (var language in necessaryLanguagesSet)
                    {
                        if (Array.IndexOf(supportLanguages, language) < 0)
                            this.m_SupportLanguages.InsertArrayEnumElement(0, language);
                    }
                }
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.m_PrintMissingLanguageLogs);
            EditorGUILayout.PropertyField(this.m_PrintMissingKeyLogs);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.m_AutoUpdateLocalizedTextOnEditor);
            EditorGUI.indentLevel--;

            this.serializedObject.ApplyModifiedProperties();
        }

        private static readonly HashSet<SystemLanguage> AllLanguagesSet = new()
        {
            SystemLanguage.Afrikaans,
            SystemLanguage.Arabic,
            SystemLanguage.Belarusian,
            SystemLanguage.Bulgarian,
            SystemLanguage.Catalan,
            SystemLanguage.Czech,
            SystemLanguage.Danish,
            SystemLanguage.German,
            SystemLanguage.Greek,
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.Estonian,
            SystemLanguage.Basque,
            SystemLanguage.Finnish,
            SystemLanguage.Faroese,
            SystemLanguage.French,
            SystemLanguage.Hebrew,
            SystemLanguage.Hindi,
            SystemLanguage.Hungarian,
            SystemLanguage.Indonesian,
            SystemLanguage.Icelandic,
            SystemLanguage.Italian,
            SystemLanguage.Japanese,
            SystemLanguage.Korean,
            SystemLanguage.Lithuanian,
            SystemLanguage.Latvian,
            SystemLanguage.Dutch,
            SystemLanguage.Norwegian,
            SystemLanguage.Polish,
            SystemLanguage.Portuguese,
            SystemLanguage.Romanian,
            SystemLanguage.Russian,
            SystemLanguage.SerboCroatian,
            SystemLanguage.Slovak,
            SystemLanguage.Slovenian,
            SystemLanguage.Swedish,
            SystemLanguage.Thai,
            SystemLanguage.Turkish,
            SystemLanguage.Ukrainian,
            SystemLanguage.Vietnamese,
            SystemLanguage.ChineseSimplified,
            SystemLanguage.ChineseTraditional,
        };
    }
}
