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

        public static readonly string ShowSelectedOverlappedCellsKey = $"{BaseKey}.{nameof(ShowSelectedOverlappedCells)}";
        public static bool ShowSelectedOverlappedCells
        {
            get => EditorPrefs.GetBool(ShowSelectedOverlappedCellsKey, false);
            set => EditorPrefs.SetBool(ShowSelectedOverlappedCellsKey, value);
        }
    }
}
#endif
