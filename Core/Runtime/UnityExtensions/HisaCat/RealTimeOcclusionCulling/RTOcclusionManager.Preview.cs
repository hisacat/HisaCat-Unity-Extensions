
using HisaCat.HUE.Collections;
using HisaCat.HUE.UnityExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling
{
    public static partial class RTOcclusionManager
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        private static void OnEnterPlaymodeInEditor_Preview(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                IsPreviewMode = false;
            }
        }
#pragma warning restore IDE0051
#endif

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void OnInitializeOnLoad()
        {
            UnityEditor.SceneView.duringSceneGui -= OnSceneGUI;
            UnityEditor.SceneView.duringSceneGui += OnSceneGUI;
        }
        private static void OnSceneGUI(UnityEditor.SceneView sceneView)
        {
            if (Application.isPlaying) return;

            if (IsPreviewMode)
            {
                UpdateCullingIfRequired();
            }
            else
            {
                // Force off culling once if preview mode is disabled.
                if (WillUpdateCulling)
                {
                    using (var enumerator = occludees.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var occludee = enumerator.Current;
                            occludee.SetCulling(false);
                        }
                    }
                    WillUpdateCulling = false;
                }
            }
        }
#endif

        private static bool _isPreviewMode = false;
        public static bool IsPreviewMode
        {
            get => _isPreviewMode;
            set
            {
                if (ConditionLog.LogError(Application.isPlaying, "Change preview mode in play mode is not supported."))
                    return;

                _isPreviewMode = value;
                OnPreviewModeChanged(value);
            }
        }
        private static void OnPreviewModeChanged(bool value)
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif

            MarkAsUpdateCulling();
        }
    }
}
