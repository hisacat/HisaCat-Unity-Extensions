
using UnityEngine;

namespace HisaCat.HUE.UnityExtensions
{
    public static class QualitySettingsHelper
    {
        public enum VSyncCount
        {
            DontSync = 0,
            EveryVBlank = 1,
            EverySecondVBlank = 2,
        }
        public static void SetVSyncCount(VSyncCount vSyncCount)
        {
            QualitySettings.vSyncCount = (int)vSyncCount;
        }
    }
}
