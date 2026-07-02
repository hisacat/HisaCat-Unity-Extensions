using System.Collections;
using HisaCat.HUE.Assets;
using HisaCat.PropertyAttributes;
using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    public class I18NFontInitializer : MonoBehaviour
    {
        [SerializeField] private I18NFontDataListAsset m_I18NFontDataListAsset = null;
        [ReadOnly][SerializeField] private string m_I18NFontDataListAssetKey = null;
        [SerializeField] private bool AlwaysInitializeAddressablesFontAsynchronously = false;

        public bool IsAddressablesFontInitialized { get; private set; } = false;
        private void Awake()
        {
            var assetOnBuild = this.m_I18NFontDataListAsset;
            InitializeAll(assetOnBuild);

            if (string.IsNullOrEmpty(this.m_I18NFontDataListAssetKey) == false)
            {
                StartCoroutine(InitializeAddressablesRoutine());
                IEnumerator InitializeAddressablesRoutine()
                {
                    I18NFontDataListAsset assetOnBundle = null;

                    // In WebGL, LoadSync is not supported so always load font asynchronously.
#if UNITY_WEBGL
                    var assetOnBundleOp = AssetLoader.Addressables.LoadAsync<I18NFontDataListAsset>(this.m_I18NFontDataListAssetKey);
                    yield return assetOnBundleOp;
                    assetOnBundle = assetOnBundleOp.Result;
#else
                    if (this.AlwaysInitializeAddressablesFontAsynchronously)
                    {
                        var assetOnBundleOp = AssetLoader.Addressables.LoadAsync<I18NFontDataListAsset>(this.m_I18NFontDataListAssetKey);
                        yield return assetOnBundleOp;
                        assetOnBundle = assetOnBundleOp.Result;
                    }
                    else
                    {
                        assetOnBundle = AssetLoader.Addressables.LoadSync<I18NFontDataListAsset>(this.m_I18NFontDataListAssetKey);
                    }
#endif

                    if (assetOnBundle == null)
                    {
                        Debug.LogError($"[{nameof(I18NFontInitializer)}] I18N Font Data List Asset not found in bundle! (Key: {this.m_I18NFontDataListAssetKey})");
                    }
                    else
                    {
                        // If the asset in the build and the asset in the bundle are different instances,
                        // each instance must be initialized.
                        // (This handles duplicated assets caused by Addressables’ duplicate bundle dependencies.)
#if UNITY_6000_4_OR_NEWER
                        // GetInstanceID is deprecated in Unity 6.4 or newer.
                        // https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Object.GetInstanceID.html
                        if (assetOnBundle != null && assetOnBuild.GetEntityId() != assetOnBundle.GetInstanceID())
#else
                        if (assetOnBundle != null && assetOnBuild.GetInstanceID() != assetOnBundle.GetInstanceID())
#endif
                        {
                            InitializeAll(assetOnBundle);
                            this.IsAddressablesFontInitialized = true;
                        }
                    }
                }

            }

            static void InitializeAll(I18NFontDataListAsset asset)
            {
                asset.InitializeAll();
#if UNITY_6000_4_OR_NEWER
                // GetInstanceID is deprecated in Unity 6.4 or newer.
                // https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Object.GetInstanceID.html
                Debug.Log($"[{nameof(I18NFontInitializer)}] Initialize {asset.name} ({asset.GetEntityId()})");
#else
                Debug.Log($"[{nameof(I18NFontInitializer)}] Initialize {asset.name} ({asset.GetInstanceID()})");
#endif
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
