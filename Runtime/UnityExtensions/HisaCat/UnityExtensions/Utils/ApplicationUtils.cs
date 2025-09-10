using UnityEngine;

namespace HisaCat.Utilities
{
    public static class ApplicationUtils
    {
        private static bool isQuitting = false;

#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                Initialize();
            }
        }
#pragma warning restore IDE0051
#endif

#pragma warning disable IDE0051
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            Initialize();
        }
#pragma warning restore IDE0051

        private static void Initialize()
        {
            isQuitting = false;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnEditorApplicationPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnEditorApplicationPlayModeStateChanged;
#else
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
#endif
        }


#if UNITY_EDITOR
        private static void OnEditorApplicationPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                isQuitting = true;
            }
        }
#else
        private static void OnApplicationQuitting()
        {
            isQuitting = true;
        }
#endif

        #region Public Methods
        /// <summary>
        /// Returns if the application is quitting or editor application is exiting play mode.<br/>
        /// If editor application currently not in play mode, returns true.
        /// </summary>
        public static bool IsQuitting()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
                return true;
#endif
            return isQuitting;
        }

        /// <summary>
        /// Returns application is playing or editor application is playing in editor.
        /// </summary>
        public static bool IsPlaying()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.isPlaying;
#else
            return Application.isPlaying;
#endif
        }

        /// <summary>
        /// Quit the application or exit play mode in editor.
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion Public Methods
    }
}
