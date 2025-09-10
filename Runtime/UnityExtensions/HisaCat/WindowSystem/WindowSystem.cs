using HisaCat;
using HisaCat.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Window
{
    public abstract class WindowSystemBase : MonoBehaviour
    {
        #region Abstract Methods
        /// <summary>
        /// Delegate and Undelegate the UINavigatePerformed event.
        /// </summary>
        protected delegate void UINavigatePerformedDelegate();
        /// <summary>
        /// Delegate the UINavigatePerformed event.
        /// </summary>
        /// <param name="callback"></param>
        protected abstract void DelegateUINavigatePerformed(UINavigatePerformedDelegate callback);
        /// <summary>
        /// Undelegate the UINavigatePerformed event.
        /// </summary>
        /// <param name="callback"></param>
        protected abstract void UndelegateUINavigatePerformed(UINavigatePerformedDelegate callback);
        /// <summary>
        /// Get the default window path.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract string GetDefaultWindowPath(System.Type type);
        /// <summary>
        /// Load the window base prefab from the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract WindowBase LoadWindowBasePrefabFrom(string path);
        /// <summary>
        /// Get the cancel button down for invoke OnBackButton method.
        /// </summary>
        /// <returns></returns>
        protected abstract bool GetCancelButtonDown();
        #endregion

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
            Instance.Init();
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Dispose();
            }
        }
        #endregion

        public static bool InstanceExist { get { return Instance != null; } }

        public RectTransform WindowArea { get { return m_windowArea; } }
        [SerializeField] private RectTransform m_windowArea = null;

        private List<WindowBase> windowList = null;

        public delegate void OnWindowFocusChangedDelegate(WindowBase prevWindow, WindowBase newWindow);
        public static event OnWindowFocusChangedDelegate OnWindowFocusChanged = null;
        public delegate void OnWindowStartShowDelegate(WindowBase window);
        public static event OnWindowStartShowDelegate OnWindowStartShow = null;
        public delegate void OnWindowShownDelegate(WindowBase window);
        public static event OnWindowShownDelegate OnWindowShown = null;

        private readonly Dictionary<string, System.Type> windowTypesDic = new();
        public static Dictionary<string, System.Type> WindowTypesDic { get { return Instance == null ? null : Instance.windowTypesDic; } }

        private readonly Dictionary<string, WindowBase> resourcesWindowCache = new();

        #region Reflection
        private void LoadAllWindowTypes()
        {
            var windowBaseType = typeof(WindowBase);
            var allWindowAssemblyTypes = windowBaseType.Assembly.GetTypes();
            int count = allWindowAssemblyTypes.Length;
            for (int i = 0; i < count; i++)
            {
                var type = allWindowAssemblyTypes[i];
                if (type.IsSubclassOf(windowBaseType))
                {
                    this.windowTypesDic.Add(type.Name, type);
                }
            }
        }
        #endregion

        private void Init()
        {
            LoadAllWindowTypes();

            windowList = new List<WindowBase>();

            // Destroy all windows currently on scene.
            var allWindows = m_windowArea.GetComponentsInChildren<WindowBase>(true);

            int windowCount = allWindows.Length;
            for (int i = 0; i < windowCount; i++)
                Destroy(allWindows[i].gameObject);

            DelegateUINavigatePerformed(OnUINavigatePerformed);
        }
        private void Dispose()
        {
            UndelegateUINavigatePerformed(OnUINavigatePerformed);
        }

        private void OnUINavigatePerformed()
        {
            // If there is no currently selected object, select the entry Selectable of the focused window.
            // When this function is called by a navigation key input,
            // the navigation event is processed again immediately after selecting the Selectable via SelectEntrySelectable,
            // which may cause the focus to move unintentionally.
            // To prevent this, we are going to select the entry Selectable after the current frame ends.
            CoroutineAction.WaitForEndOfFrame(Process);
            static void Process()
            {
                if (EventSystem.current == null) return;
                if (EventSystem.current.currentSelectedGameObject != null) return;

                var focusedWindow = GetFocusedWindow();
                if (focusedWindow == null) return;

                focusedWindow.TrySelectEntrySelectable();
            }
        }

        public static bool IsFocused<T>(T window) where T : WindowBase
        {
            if (Instance.windowList.Count > 0)
                return Instance.windowList[Instance.windowList.Count - 1] == window;
            else
                return false;
        }
        public static bool IsWindowTypeFocused<T>() where T : WindowBase
        {
            if (Instance.windowList.Count > 0)
                return Instance.windowList[Instance.windowList.Count - 1].GetType() == typeof(T);
            else
                return false;
        }

        public static WindowBase GetShownWindowHasInterface<T>() where T : class
        {
            if (!typeof(T).IsInterface)
                throw new System.Exception($"{typeof(T).Name} is not interface!");

            return Instance.windowList.FirstOrDefault(x => x.IsShown && x is T);
        }

        public static T FindAliveWindowOfType<T>() where T : WindowBase
            => Instance.windowList.Find(x => x.GetType() == typeof(T)) as T;
        public static WindowBase FindAliveWindowOfType(System.Type type)
            => Instance.windowList.Find(x => x.GetType() == type);

        public static bool IsAliveWindowExistOfType<T>() where T : WindowBase
            => FindAliveWindowOfType<T>() != null;
        public static bool IsAliveWindowExistOfType(System.Type type)
            => FindAliveWindowOfType(type) != null;

        public static string GetDefaultWindowPath<T>() where T : WindowBase
            => GetDefaultWindowPath_Internal(typeof(T));
        public static string GetDefaultWindowPath_Internal(System.Type type)
            => InstanceExist ? Instance.GetDefaultWindowPath(type) : throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(GetDefaultWindowPath_Internal)}: Instance is not initialized yet.");

        public static T LoadWindowPrefab<T>(string specialPath = null) where T : WindowBase
            => LoadWindowPrefab(typeof(T), specialPath) as T;
        public static WindowBase LoadWindowPrefab(System.Type type, string specialPath = null)
        {
            var windowPath = string.IsNullOrEmpty(specialPath) ?
                GetDefaultWindowPath_Internal(type) : $"Windows/{specialPath}";

            if (Instance.resourcesWindowCache.ContainsKey(windowPath))
                return Instance.resourcesWindowCache[windowPath];

            var window = LoadWindowBasePrefabFrom_Internal(windowPath);
            var loadedWindowType = window.GetType();
            if (window != null && loadedWindowType != type)
            {
                Debug.LogError(
                    $"[{nameof(WindowSystemBase)}] {nameof(LoadWindowPrefab)}: Window '{window.name}' type '{loadedWindowType.Name}' is not '{type.Name}'."
                    + $"\r\nPath: '{windowPath}'");
                return null;
            }

            Instance.resourcesWindowCache.Add(windowPath, window);
            return window;
        }

        private static WindowBase LoadWindowBasePrefabFrom_Internal(string path)
            => InstanceExist ? Instance.LoadWindowBasePrefabFrom(path) : throw new System.InvalidOperationException($"[{nameof(WindowSystemBase)}] {nameof(LoadWindowBasePrefabFrom_Internal)}: Instance is not initialized yet.");


        public static WindowBase ShowNewInstance(System.Type type, string specialPath = null, bool immediately = false)
        {
            WindowBase windowResource = LoadWindowPrefab(type, specialPath);
            if (windowResource == null)
            {
                Debug.LogError(
                    $"[{nameof(WindowSystemBase)}] {nameof(ShowNewInstance)}: Cannot find window '{type.Name}'." +
                    $"\r\nPath: '{(specialPath == null ? GetDefaultWindowPath_Internal(type) : specialPath)}'");
                return null;
            }

            var window = Instantiate(windowResource, Instance.WindowArea.transform);
            window.name = windowResource.name;
            window.DestroyOnClosed = true;

            if (Show(window, immediately))
                return window;
            else
                return null;
        }
        public static T ShowNewInstance<T>(string specialPath = null, bool immediately = false) where T : WindowBase
            => ShowNewInstance(typeof(T), specialPath, immediately) as T;

        public static bool Show<T>(T window, bool immediately = false) where T : WindowBase
        {
            if (window.isShowing || window.IsShown)
            {
                ManagedDebug.LogWarningFormat($"[{nameof(WindowSystemBase)}] Window {0} is already showing or shown", window.name);
                return false;
            }
            Instance.StartCoroutine(Instance.ShowRoutine(window, immediately));

            return true;
        }
        public static bool Close<T>(T window, bool immediately = false) where T : WindowBase
        {
            if (window.IsClosing || window.IsClosed)
            {
                ManagedDebug.LogWarning($"[{nameof(WindowSystemBase)}] Window {window.name} is already closing or closed");
                return false;
            }
            Instance.StartCoroutine(Instance.CloseRoutine(window, immediately));

            return true;
        }
        public static void CloseAllWindows(bool immediately = false)
        {
            var windowCount = Instance.windowList.Count;
            for (int i = windowCount - 1; i >= 0; i--)
                Close(Instance.windowList[i], immediately);
        }
        public static void CloseAllWindows(HashSet<WindowBase> except = null, bool immediately = false)
        {
            var windowCount = Instance.windowList.Count;
            for (int i = windowCount - 1; i >= 0; i--)
            {
                var window = Instance.windowList[i];
                if (except.Contains(window)) continue;
                Close(window, immediately);
            }
        }

        private IEnumerator ShowRoutine<T>(T window, bool immediately) where T : WindowBase
        {
            var prevWindow = windowList.Count <= 0 ? null : windowList[windowList.Count - 1];
            if (prevWindow != null) prevWindow.OnLostFocusedCallback();

            window.gameObject.SetActive(true);

            windowList.Add(window);
            UpdateWindowSiblingIndex();

            window.OnStartShowCallback();
            OnWindowStartShow?.Invoke(window);

            window.OnFocusedCallback();
            OnWindowFocusChanged?.Invoke(prevWindow, window);

            var anim = window.WindowAnimation;
            if (immediately == false && anim != null && window.ShowAnimationClip != null)
            {
                anim.Stop();
                anim.AddClip(window.ShowAnimationClip, window.ShowAnimationClip.name);

                anim.clip = window.ShowAnimationClip;
                anim.Play();

                // Check window was destoyed: (anim != null)
                while (anim != null && anim.isPlaying) yield return null;
                if (anim != null) anim.clip = null;
                yield return CachedYieldInstruction.WaitForEndOfFrame();
            }

            window.OnShownCallback();
            OnWindowShown?.Invoke(window);
        }
        private IEnumerator CloseRoutine<T>(T window, bool immediately) where T : WindowBase
        {
            window.OnLostFocusedCallback();
            window.OnStartCloseCallback();

            windowList.Remove(window);

            if (windowList.Count > 0)
            {
                var newFocusWindow = windowList[windowList.Count - 1];
                newFocusWindow.OnFocusedCallback();
                OnWindowFocusChanged?.Invoke(window, newFocusWindow);
            }
            else
            {
                OnWindowFocusChanged?.Invoke(window, null);
            }

            if (immediately == false)
            {
                // Some window can be inactivated. in this case, the animation wont play so window does not closed.
                // So close without animation when widnow is inactivated.
                if (window.gameObject.activeInHierarchy)
                {
                    var anim = window.WindowAnimation;
                    if (anim != null && window.CloseAnimationClip != null)
                    {
                        anim.Stop();

                        anim.AddClip(window.CloseAnimationClip, window.CloseAnimationClip.name);

                        anim.clip = window.CloseAnimationClip;
                        anim.Play();

                        while (anim.isPlaying) yield return null;
                        if (anim != null) anim.clip = null;
                        yield return CachedYieldInstruction.WaitForEndOfFrame();
                    }
                }
            }

            window.OnClosedCallback();
            window.gameObject.SetActive(false);

            if (window.DestroyOnClosed)
            {
                Destroy(window.gameObject);
            }
        }

        public static void Focus(WindowBase window)
        {
            if (IsFocused(window))
            {
                ManagedDebug.LogWarning($"[{nameof(WindowSystemBase)}] Window {window.name} is already focused");
                return;
            }

            if (Instance.windowList.Contains(window) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] Cannot focus window {window.name}. it not exist");
                return;
            }
            else
            {
                var prevFocusedWindow = GetFocusedWindow();
                {
                    Instance.windowList.Remove(window);
                    Instance.windowList.Add(window);

                    UpdateWindowSiblingIndex();
                }
                var currentFocusedWindow = GetFocusedWindow();

                if (prevFocusedWindow != currentFocusedWindow)
                {
                    prevFocusedWindow.OnLostFocusedCallback();
                    currentFocusedWindow.OnFocusedCallback();
                    OnWindowFocusChanged?.Invoke(prevFocusedWindow, currentFocusedWindow);
                }
            }
        }

        public static void SendToBack(WindowBase window)
        {
            if (Instance.windowList.Contains(window) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] {nameof(SendToBack)}: Window {window.name} not exist");
                return;
            }

            var prevFocusedWindow = GetFocusedWindow();
            {
                Instance.windowList.Remove(window);
                Instance.windowList.Insert(0, window);

                UpdateWindowSiblingIndex();
            }
            var currentFocusedWindow = GetFocusedWindow();

            if (prevFocusedWindow != currentFocusedWindow)
            {
                prevFocusedWindow.OnLostFocusedCallback();
                currentFocusedWindow.OnFocusedCallback();
                OnWindowFocusChanged?.Invoke(prevFocusedWindow, currentFocusedWindow);
            }
        }
        [System.Obsolete("Use Focus instead.")]
        public static void BringToFront(WindowBase window) => Focus(window);

        public static void SendToBehindOf(WindowBase window, WindowBase from)
        {
            if (Instance.windowList.Contains(window) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] {nameof(SendToBack)}: Window {window.name} not exist");
                return;
            }
            if (Instance.windowList.Contains(from) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] {nameof(SendToBack)}: Window {from.name} not exist");
                return;
            }

            var prevFocusedWindow = GetFocusedWindow();
            {
                Instance.windowList.Remove(window);
                var index = Instance.windowList.IndexOf(from);
                Instance.windowList.Insert(index, window);

                UpdateWindowSiblingIndex();
            }
            var currentFocusedWindow = GetFocusedWindow();

            if (prevFocusedWindow != currentFocusedWindow)
            {
                prevFocusedWindow.OnLostFocusedCallback();
                currentFocusedWindow.OnFocusedCallback();
                OnWindowFocusChanged?.Invoke(prevFocusedWindow, currentFocusedWindow);
            }
        }
        public static void BringToFrontOf(WindowBase window, WindowBase from)
        {
            if (Instance.windowList.Contains(window) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] {nameof(SendToBack)}: Window {window.name} not exist");
                return;
            }
            if (Instance.windowList.Contains(from) == false)
            {
                ManagedDebug.LogError($"[{nameof(WindowSystemBase)}] {nameof(SendToBack)}: Window {from.name} not exist");
                return;
            }

            var prevFocusedWindow = GetFocusedWindow();
            {
                Instance.windowList.Remove(window);
                var index = Instance.windowList.IndexOf(from) + 1;
                Instance.windowList.Insert(index, window);

                UpdateWindowSiblingIndex();
            }
            var currentFocusedWindow = GetFocusedWindow();

            if (prevFocusedWindow != currentFocusedWindow)
            {
                prevFocusedWindow.OnLostFocusedCallback();
                currentFocusedWindow.OnFocusedCallback();
                OnWindowFocusChanged?.Invoke(prevFocusedWindow, currentFocusedWindow);
            }
        }

        public static IReadOnlyList<WindowBase> GetAllAliveWindows()
        {
            return Instance.windowList.AsReadOnly();
        }

        public static WindowBase GetFocusedWindow()
        {
            if (Instance.windowList.Count > 0)
                return Instance.windowList[Instance.windowList.Count - 1];
            else
                return null;
        }

        public static void UpdateWindowSiblingIndex()
        {
            int count = Instance.windowList.Count;
            for (int i = 0; i < count; i++)
                Instance.windowList[i].transform.SetAsLastSibling();

            for (int i = 0; i < count; i++)
            {
                if (Instance.windowList[i].AlwaysOnTop)
                    Instance.windowList[i].transform.SetAsLastSibling();
            }
        }

        private void Update()
        {
            if (GetCancelButtonDown())
            {
                if (windowList.Count > 0)
                    windowList[windowList.Count - 1].OnBackButton();
            }
        }
    }
}
