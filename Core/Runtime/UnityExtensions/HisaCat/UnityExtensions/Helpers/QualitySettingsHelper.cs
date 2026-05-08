
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVSyncCount(VSyncCount vSyncCount)
        {
            QualitySettings.vSyncCount = (int)vSyncCount;
        }
    }
}
