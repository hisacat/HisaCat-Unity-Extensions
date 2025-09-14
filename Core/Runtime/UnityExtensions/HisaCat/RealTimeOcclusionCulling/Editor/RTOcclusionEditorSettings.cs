#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static class RTOcclusionEditorSettings
    {
        private static readonly string BaseKey = $"{nameof(RTOcclusionEditorSettings)}";
        private static readonly string ShowGizmosAlwaysKey = $"{BaseKey}.{nameof(ShowGizmosAlways)}";
        public static bool ShowGizmosAlways
        {
            get => EditorPrefs.GetBool(ShowGizmosAlwaysKey, false);
            set => EditorPrefs.SetBool(ShowGizmosAlwaysKey, value);
        }
    }
}
#endif
