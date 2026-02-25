using HisaCat.IO;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.Localization
{
    [DisallowMultipleComponent]
    public class LocalizationSettingsAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("HisaCat/Localization/Create Localization Settings Asset")]
        public static void CreateSettingsAsset()
        {
            var path = UniPath.Combine("Assets", "Resources", $"{DefaultAssetPath}.asset");
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<LocalizationSettingsAsset>(path);
            if (asset != null)
            {
                UnityEditor.EditorGUIUtility.PingObject(asset);
                Debug.Log($"[{nameof(LocalizationSettingsAsset)}] Settings asset already exists at \"{path}\".");
                return;
            }

            asset = CreateDefaultInstance();
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(asset, path);

            UnityEditor.EditorGUIUtility.PingObject(asset);
            Debug.Log($"[{nameof(LocalizationSettingsAsset)}] Settings asset created at \"{path}\".");
        }
#endif

        public const string DefaultAssetPath = "Localization Settings";

        // public const string DefulatLocalizedJsonsPath = "Localization";
        // [SerializeField] private string m_LocalizedJsonsPath = DefulatLocalizedJsonsPath; public string LocalizedJsonsPath => this.m_LocalizedJsonsPath;

        [SerializeField] private SystemLanguage m_DefaultLanguage = SystemLanguage.English; public SystemLanguage DefaultLanguage => this.m_DefaultLanguage;
        [SerializeField] private SystemLanguage m_FallbackLanguage = SystemLanguage.English; public SystemLanguage FallbackLanguage => this.m_FallbackLanguage;
        [SerializeField] private List<SystemLanguage> m_SupportLanguages = new() { SystemLanguage.English };
        private HashSet<SystemLanguage> _supportLanguages = null;
        public HashSet<SystemLanguage> SupportLanguages => _supportLanguages ??= new HashSet<SystemLanguage>(this.m_SupportLanguages);

        [SerializeField] private bool m_PrintMissingLanguageLogs = true; public bool PrintMissingLanguageLogs => this.m_PrintMissingLanguageLogs;
        [SerializeField] private bool m_PrintMissingKeyLogs = true; public bool PrintMissingKeyLogs => this.m_PrintMissingKeyLogs;

        [SerializeField] private bool m_AutoUpdateLocalizedTextOnEditor = true; public bool AutoUpdateLocalizedTextOnEditor => this.m_AutoUpdateLocalizedTextOnEditor;

        public static LocalizationSettingsAsset CreateDefaultInstance()
        {
            var asset = CreateInstance<LocalizationSettingsAsset>();
            return asset;
        }

        private void OnValidate()
        {
            this._supportLanguages = null;
        }
    }
}
