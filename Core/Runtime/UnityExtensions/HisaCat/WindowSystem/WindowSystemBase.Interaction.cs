using UnityEngine;
using UnityEngine.EventSystems;

namespace HisaCat.HUE.UI.Windows
{
    public abstract partial class WindowSystemBase : MonoBehaviour
    {
        /// <summary>
        /// Callback of UI Navigate performed event.<br/>
        /// It selects the entry Selectable of the focused window internally.
        /// </summary>
        public void OnUINavigatePerformed()
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

        /// <summary>
        /// Callback of Back (UI Cancel) performed event.<br/>
        /// Invoke OnBackButton method of the focused window.
        /// </summary>
        public void OnBackButton()
        {
            if (windowList.Count > 0)
                windowList[^1].OnBackButton();
        }
    }
}
