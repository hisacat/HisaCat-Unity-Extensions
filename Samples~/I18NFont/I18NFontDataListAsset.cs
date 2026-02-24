using UnityEngine;

namespace HisaCat.HUE.Fonts
{
    [CreateAssetMenu(fileName = "I18NFontDataListAsset", menuName = "HisaCat/HUE/I18N Fonts/I18N Font Data List Asset", order = 1)]
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
