using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

namespace HisaCat.HUE.Settings
{
    public partial class HueSettings : ScriptableObject
    {
        public const string AssetPath = "HueSettings/HueSettings.asset";
        private static HueSettings _instance = null;
        public static HueSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (File.Exists(AssetPath) == false)
                    {
                        var directory = Path.GetDirectoryName(AssetPath);
                        if (Directory.Exists(directory) == false) Directory.CreateDirectory(directory);
                        _instance = CreateInstance<HueSettings>();
                        Save();
                        return _instance;
                    }
                    var assets = InternalEditorUtility.LoadSerializedFileAndForget(AssetPath);
                    if (assets.Length <= 0) ThrowLoadFailedException(AssetPath);
                    _instance = assets[0] as HueSettings;
                    if (_instance == null) ThrowLoadFailedException(AssetPath);
                    static void ThrowLoadFailedException(string assetPath) => throw new System.Exception($"[{nameof(HueSettings)}] {nameof(Instance)}: Failed to load asset at \"{assetPath}\".");
                }
                return _instance;
            }
        }

        public static void Save()
        {
            var directory = Path.GetDirectoryName(AssetPath);
            if (Directory.Exists(directory) == false) Directory.CreateDirectory(directory);
            EditorUtility.SetDirty(Instance);
            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { Instance }, AssetPath, true);
        }

        public GizmosSettingsData GizmosSettings { get => this.m_GizmosConfig; set => this.m_GizmosConfig = value; }
        [SerializeField] private GizmosSettingsData m_GizmosConfig = new();
        [System.Serializable]
        public class GizmosSettingsData
        {
            public MeshColliderSettingsData MeshColliderSettings { get => this.m_MeshColliderSettings; set => this.m_MeshColliderSettings = value; }
            [SerializeField] private MeshColliderSettingsData m_MeshColliderSettings = new();
            [System.Serializable]
            public class MeshColliderSettingsData
            {
                [SerializeField] private bool m_ShowNonConvexWithRenderer = false;
                public bool ShowNonConvexWithRenderer { get => this.m_ShowNonConvexWithRenderer; set => this.m_ShowNonConvexWithRenderer = value; }
                public static readonly Color DefaultNonConvexWithRendererColor = new Color32(145, 244, 139, 192);
                [SerializeField] private Color m_NonConvexWithRendererColor = DefaultNonConvexWithRendererColor;
                public Color NonConvexWithRendererColor { get => this.m_NonConvexWithRendererColor; set => this.m_NonConvexWithRendererColor = value; }
            }
        }
    }
}
