using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HisaCat.HUE.UI.Windows
{
    [DefaultExecutionOrder(int.MinValue)]
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                Instance = null;
                OnWindowFocusChanged = null;
                OnWindowStartShow = null;
                OnWindowShown = null;
            }
        }
#pragma warning restore IDE0051
#endif

        #region Singleton
        private static WindowSystemBase Instance { get; set; } = null;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Instance.Init_Internal();
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Dispose_Internal();
            }
        }
        #endregion
        private void Init_Internal()
        {
            LoadAllWindowTypes();

            windowList = new List<WindowBase>();

            // Destroy all windows currently on scene.
            var allWindows = this.m_windowArea.GetComponentsInChildren<WindowBase>(true);

            int windowCount = allWindows.Length;
            for (int i = 0; i < windowCount; i++)
                Destroy(allWindows[i].gameObject);

            Init();
        }
        protected virtual void Init() { }

        private void Dispose_Internal()
        {
            this.Dispose();
        }
        protected virtual void Dispose() { }

        public static bool InstanceExist { get { return Instance != null; } }
        public RectTransform WindowArea { get { return m_windowArea; } }
        [SerializeField] private RectTransform m_windowArea = null;

        private List<WindowBase> windowList = null;

        private readonly Dictionary<string, System.Type> windowTypesDic = new();
        public static Dictionary<string, System.Type> WindowTypesDic { get { return Instance == null ? null : Instance.windowTypesDic; } }

        private readonly Dictionary<string, WindowBase> loadedWindowPrefabsCache = new();

        #region Reflection
        private void LoadAllWindowTypes()
        {
            var windowBaseType = typeof(WindowBase);

            foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(windowBaseType))
                        this.windowTypesDic.Add(type.Name, type);
                }
            }
        }
        #endregion
    }
}
