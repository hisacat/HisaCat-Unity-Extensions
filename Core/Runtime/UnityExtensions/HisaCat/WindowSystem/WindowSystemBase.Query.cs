using System.Collections.Generic;
using UnityEngine;

namespace HisaCat.HUE.UI.Windows
{
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
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

        public static IReadOnlyList<WindowBase> GetAllAliveWindows() => Instance.windowList.AsReadOnly();
        public static WindowBase GetFocusedWindow() => Instance.windowList.Count > 0 ? Instance.windowList[^1] : null;
        public static WindowBase GetShownWindowHasInterface<T>() where T : class
        {
            if (typeof(T).IsInterface == false)
                throw new System.Exception($"{typeof(T).Name} is not interface!");

            // return Instance.windowList.FirstOrDefault(x => x.IsShown && x is T);
            foreach (var window in Instance.windowList)
            {
                if (window.IsShown && window is T)
                    return window;
            }
            return null;
        }

        public static T FindAliveWindowOfType<T>() where T : WindowBase
            => Instance.windowList.Find(x => x.GetType() == typeof(T)) as T;
        public static WindowBase FindAliveWindowOfType(System.Type type)
            => Instance.windowList.Find(x => x.GetType() == type);

        public static bool IsAliveWindowExistOfType<T>() where T : WindowBase
            => FindAliveWindowOfType<T>() != null;
        public static bool IsAliveWindowExistOfType(System.Type type)
            => FindAliveWindowOfType(type) != null;
    }
}
