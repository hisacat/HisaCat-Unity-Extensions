#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HisaCat.Localization
{
    public class LocalizationSettingsWindow : EditorWindow
    {
        [MenuItem("HisaCat/Localization/Localization Settings", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationSettingsWindow>("Localization Settings");
            window.maxSize = new Vector2(400, 200); //(400, 800);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Localization Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            var selectedLangauge = (SystemLanguage)EditorGUILayout.EnumPopup("Selected langauge", LocalizationManager.SelectedLanguage);
            if (EditorGUI.EndChangeCheck())
                LocalizationManager.SelectedLanguage = selectedLangauge;
            if (GUILayout.Button($"Reset to Default language ({LocalizationSettings.DefaultLanguage})"))
                LocalizationManager.SelectedLanguage = LocalizationSettings.DefaultLanguage;
        }
    }
}
#endif
