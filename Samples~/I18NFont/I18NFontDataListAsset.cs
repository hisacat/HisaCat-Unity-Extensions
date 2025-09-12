using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    [CreateAssetMenu(fileName = "I18NFontDataListAsset", menuName = "HisaCat/I18NFontDataListAsset", order = 1)]
    public class I18NFontDataListAsset : ScriptableObject
    {
        public I18NFontDataAsset[] I18NFontDataAssets => this.m_I18NFontDataAssets;
        [SerializeField] private I18NFontDataAsset[] m_I18NFontDataAssets = null;

        public void InitializeAll()
        {
            foreach (var i18nFontDataAsset in this.m_I18NFontDataAssets)
                i18nFontDataAsset.Initialize();
        }
    }
}
