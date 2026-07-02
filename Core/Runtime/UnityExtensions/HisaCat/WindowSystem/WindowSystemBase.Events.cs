using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HisaCat.HUE.UI.Windows
{
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
        public delegate void OnWindowFocusChangedDelegate(WindowBase prevWindow, WindowBase newWindow);
        public static event OnWindowFocusChangedDelegate OnWindowFocusChanged = null;
        public delegate void OnWindowStartShowDelegate(WindowBase window);
        public static event OnWindowStartShowDelegate OnWindowStartShow = null;
        public delegate void OnWindowShownDelegate(WindowBase window);
        public static event OnWindowShownDelegate OnWindowShown = null;
    }
}
