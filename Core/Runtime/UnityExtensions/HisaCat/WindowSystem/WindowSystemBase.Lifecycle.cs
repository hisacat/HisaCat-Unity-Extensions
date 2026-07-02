using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.UI.Windows
{
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
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
        public static void CloseAllWindows(HashSet<WindowBase> except, bool immediately = false)
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
                if (window.UseUnscaledTimeForAnimation)
                {
                    yield return anim.PlayUnscaledTimeRoutine(window.ShowAnimationClip.name);
                }
                else
                {
                    anim.Play();
                    // Check window was destoyed: (anim != null)
                    while (anim != null && anim.isPlaying) yield return null;
                    if (anim != null) anim.clip = null;
                    yield return CachedYieldInstruction.WaitForEndOfFrame();
                }
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
                        if (window.UseUnscaledTimeForAnimation)
                        {
                            yield return anim.PlayUnscaledTimeRoutine(window.CloseAnimationClip.name);
                        }
                        else
                        {
                            anim.Play();
                            while (anim.isPlaying) yield return null;
                            if (anim != null) anim.clip = null;
                            yield return CachedYieldInstruction.WaitForEndOfFrame();
                        }
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
    }
}
