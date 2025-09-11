using HisaCat.PropertyAttributes;
using UnityEngine;

namespace HisaCat.Fonts
{
    public class I18NFontInitializer : MonoBehaviour
    {
        [SerializeField] private I18NFontDataListAsset m_I18NFontDataListAsset = null;
        [ReadOnly][SerializeField] private string m_I18NFontDataListAssetKey = null;
        private void Awake()
        {
            var assetOnBuild = this.m_I18NFontDataListAsset;
            var assetOnBundle = string.IsNullOrEmpty(this.m_I18NFontDataListAssetKey) ? null :
            HUE.Assets.AssetLoader.Addressables.LoadSync<I18NFontDataListAsset>(this.m_I18NFontDataListAssetKey);

            InitializeAll(assetOnBuild);

            // If the Asset on Build and the Asset on Bundle are different instances, both need to be initialized.
            // (Manage duplicated Asset from Addressables duplicate bundle dependencies)
            if (assetOnBundle != null && assetOnBuild.GetInstanceID() != assetOnBundle.GetInstanceID())
                InitializeAll(assetOnBundle);

            static void InitializeAll(I18NFontDataListAsset asset)
            {
                asset.InitializeAll();
                Debug.Log($"[{nameof(I18NFontInitializer)}] Initialize {asset.name} ({asset.GetInstanceID()})");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.m_I18NFontDataListAsset == null)
            {
                this.m_I18NFontDataListAssetKey = null;
            }
            else
            {
                var group = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.DefaultGroup;
                string path = null;
                foreach (var entry in group.entries)
                {
                    if (entry.TargetAsset == this.m_I18NFontDataListAsset)
                    {
                        path = entry.AssetPath;
                        break;
                    }
                }
                this.m_I18NFontDataListAssetKey = path;
            }

        }
#endif
    }
}
