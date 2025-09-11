using System.Collections.Generic;
using UnityEngine;

namespace HisaCat
{
    public static class CachedYieldInstruction
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                _WaitForSecondsDictionary = null;
                _WaitForSecondsRealtimeDictionary = null;
                _CachedWaitForEndOfFrame = null;
                _CachedWaitForFixedUpdate = null;
            }
        }
#pragma warning restore IDE0051
#endif

        class FloatComparer : IEqualityComparer<float>
        {
            bool IEqualityComparer<float>.Equals(float x, float y) => x == y;
            int IEqualityComparer<float>.GetHashCode(float obj) => obj.GetHashCode();
        }


        private static Dictionary<float, WaitForSeconds> _WaitForSecondsDictionary = null;
        private static Dictionary<float, WaitForSeconds> WaitForSecondsDictionary => _WaitForSecondsDictionary ??= new(new FloatComparer());
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (WaitForSecondsDictionary.TryGetValue(seconds, out var value))
                return value;

            value = new UnityEngine.WaitForSeconds(seconds);
            WaitForSecondsDictionary.Add(seconds, value);
            return value;
        }
        public static WaitForSeconds WaitForOneSecond() => WaitForSeconds(1f);
        public static WaitForSeconds WaitForOneMinute() => WaitForSeconds(60f);

        private static Dictionary<float, WaitForSecondsRealtime> _WaitForSecondsRealtimeDictionary = null;
        private static Dictionary<float, WaitForSecondsRealtime> WaitForSecondsRealtimeDictionary => _WaitForSecondsRealtimeDictionary ??= new(new FloatComparer());
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            if (WaitForSecondsRealtimeDictionary.TryGetValue(seconds, out var value))
                return value;

            value = new UnityEngine.WaitForSecondsRealtime(seconds);
            WaitForSecondsRealtimeDictionary.Add(seconds, value);
            return value;
        }

        private static WaitForEndOfFrame _CachedWaitForEndOfFrame = null;
        public static WaitForEndOfFrame WaitForEndOfFrame() => _CachedWaitForEndOfFrame ??= new();

        private static WaitForFixedUpdate _CachedWaitForFixedUpdate = null;
        public static WaitForFixedUpdate WaitForFixedUpdate() => _CachedWaitForFixedUpdate ??= new();
    }
}
