using HisaCat;
using HisaCat.Sounds;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Window
{
    [RequireComponent(typeof(Animation))]
    [RequireComponent(typeof(CanvasGroup))]
    public class WindowBase : MonoBehaviour
    {
        #region Require Components
        private Animation _windowAnimation = null;
        public Animation WindowAnimation
        {
            get
            {
                if (this._windowAnimation == null) this._windowAnimation = GetComponent<Animation>();
                return this._windowAnimation;
            }
        }
        private CanvasGroup _canvasGroup = null;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (this._canvasGroup == null) this._canvasGroup = GetComponent<CanvasGroup>();
                return this._canvasGroup;
            }
        }
        #endregion Require Components

        #region Serialized Fields
        [SerializeField] private bool m_FocusOnlyInteractable = true;
        [SerializeField] private bool m_UnblocksRaycastsOnStartClose = true;

        [SerializeField] private bool m_AlwaysOnTop = false;

        [SerializeField] private Selectable m_EntrySelectable = null;
        [SerializeField] private bool m_AutoSelectEntrySelectable = false;

        [SerializeField] private AudioClip m_BGMClip = null;
        [SerializeField] private bool m_PauseBGMOnShow = false;
        [SerializeField] private bool m_PauseBGMOnFocus = false;

        [SerializeField] private AudioClip m_ShowSEClip = null;
        [SerializeField] private AudioClip m_CloseSEClip = null;

        [SerializeField] private AnimationClip m_ShowAnimationClip = null;
        [SerializeField] private AnimationClip m_CloseAnimationClip = null;
        #endregion Serialized Fields

        #region Properties
        public AnimationClip ShowAnimationClip { get { return m_ShowAnimationClip; } }
        public AnimationClip CloseAnimationClip { get { return m_CloseAnimationClip; } }

        public bool AlwaysOnTop
        {
            get { return m_AlwaysOnTop; }
            set
            {
                if (m_AlwaysOnTop != value)
                {
                    m_AlwaysOnTop = value;
                    WindowSystemBase.UpdateWindowSiblingIndex();
                }
            }
        }

        public bool DestroyOnClosed { get; set; } = false;
        #endregion Properties

        public delegate void WindowEventDelegate(WindowBase window);
        public event WindowEventDelegate onStartClose = null;
        public event WindowEventDelegate onClosed = null;

        public bool isShowing { get; private set; } = false;
        public bool IsShown { get; private set; } = false;
        public bool IsClosing { get; private set; } = false;
        public bool IsClosed { get; private set; } = false;

        private List<WindowBase> childWindows = null;

        #region Public function
        public bool Show() => WindowSystemBase.Show(this, false);
        public void Focus() => WindowSystemBase.Focus(this);
        public void SendToBack() => WindowSystemBase.SendToBack(this);
        [System.Obsolete("Use Focus instead.")]
        public void BringToFront() => Focus();
        public void SendToBehindOf(WindowBase from) => WindowSystemBase.SendToBehindOf(this, from);
        public void BringToFrontOf(WindowBase from) => WindowSystemBase.BringToFrontOf(this, from);
        public virtual bool Close(bool immediately = false) => WindowSystemBase.Close(this, immediately);
        public bool IsFocused() => WindowSystemBase.IsFocused(this);
        public void ShowChildWindow<T>(T window) where T : WindowBase
        {
            if (window.Show())
            {
                if (childWindows == null) childWindows = new List<WindowBase>();
                childWindows.Add(window);
            }
            else
            {
                ManagedDebug.LogError($"[{this.name}] Cannot show childwindow {typeof(T).Name}");
            }
        }
        public T ShowChildWindowFromResources<T>(string specialPath = null) where T : WindowBase
        {
            T window = WindowSystemBase.ShowNewInstance<T>(specialPath);

            if (window != null)
            {
                if (childWindows == null) childWindows = new List<WindowBase>();
                childWindows.Add(window);

                return window;
            }
            else
            {
                ManagedDebug.LogError($"[{this.name}] Cannot show childwindow {typeof(T).Name}");
                return null;
            }
        }
        #endregion

        #region Callbacks from WindowSystem
        private SoundManager.BGMTicket curBGMTicket = null;
        private SoundManager.BGMTicket curPauseBGMOnShowTicket = null;
        private SoundManager.BGMTicket curPauseBGMOnFocusTicket = null;
        private Coroutine autoSelectEntrySelectableCoroutine = null;
        public void OnStartShowCallback()
        {
            isShowing = true;
            IsShown = false;

            if (this.m_BGMClip != null)
                this.curBGMTicket = SoundManager.PlayBGMFromQueue(this, this.m_BGMClip);

            if (this.m_PauseBGMOnShow)
                this.curPauseBGMOnShowTicket = SoundManager.PauseBGMFromQueue(this);

            if (this.m_ShowSEClip != null)
                SoundManager.PlaySE(this.m_ShowSEClip);

            // Try select the EntrySelectable if it is set.
            if (this.m_EntrySelectable != null && this.m_AutoSelectEntrySelectable)
            {
                // Since the EntrySelectable may not be interactable yet due to animations, 
                // try to select it using a coroutine.
                if (this.autoSelectEntrySelectableCoroutine != null)
                {
                    StopCoroutine(this.autoSelectEntrySelectableCoroutine);
                    this.autoSelectEntrySelectableCoroutine = null;
                }
                this.autoSelectEntrySelectableCoroutine = StartCoroutine(AutoSelectEntrySelectableRoutine());
                IEnumerator AutoSelectEntrySelectableRoutine()
                {
                    while (true)
                    {
                        // Stop if the window is no longer available.
                        if (this == null) break;
                        if (this.IsFocused() == false) break;

                        // Stop if the EntrySelectable is no longer available.
                        if (this.m_EntrySelectable == null) break;

                        // Stop if the EntrySelectable is already selected.
                        if (EventSystem.current != null &&
                            EventSystem.current.currentSelectedGameObject != null) break;

                        // If the EntrySelectable is active and interactable, select it and stop the coroutine.
                        if (this.m_EntrySelectable.isActiveAndEnabled && this.m_EntrySelectable.IsInteractable())
                        {
                            this.m_EntrySelectable.Select();
                            yield break;
                        }

                        // Wait until the end of the frame before trying again.
                        yield return CachedYieldInstruction.WaitForEndOfFrame();
                    }
                    this.autoSelectEntrySelectableCoroutine = null;
                    yield break;
                }
            }

            OnStartShown();
        }
        public void OnShownCallback()
        {
            isShowing = false;
            IsShown = true;

            OnShown();
        }
        public void OnStartCloseCallback()
        {
            IsClosing = true;
            IsClosed = false;

            onStartClose?.Invoke(this);
            onStartClose = null;

            if (this.m_UnblocksRaycastsOnStartClose)
            {
                this.CanvasGroup.blocksRaycasts = false;
            }

            if (this.curBGMTicket != null)
            {
                SoundManager.RemoveBGMFromQueue(this.curBGMTicket);
                this.curBGMTicket = null;
            }
            if (this.curPauseBGMOnShowTicket != null)
            {
                SoundManager.RemoveBGMFromQueue(this.curPauseBGMOnShowTicket);
                this.curPauseBGMOnShowTicket = null;
            }

            if (this.m_CloseSEClip != null)
                SoundManager.PlaySE(this.m_CloseSEClip);

            OnStartClose();

            // Set ignore raycast until window closed.
            var cg = this.gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = this.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            if (childWindows != null)
            {
                int count = childWindows.Count;
                for (int i = 0; i < count; i++)
                {
                    var window = childWindows[i];
                    if (window != null)
                    {
                        if (!window.IsClosed && !window.IsClosing)
                            WindowSystemBase.Close(window);
                    }
                }
            }
        }
        public void OnClosedCallback()
        {
            IsClosing = false;
            IsClosed = true;

            OnClosed();

            onClosed?.Invoke(this);
            onClosed = null;
        }
        public void OnLostFocusedCallback()
        {
            TryDeselectIfChildSelected();

            if (this.curPauseBGMOnFocusTicket != null)
            {
                SoundManager.RemoveBGMFromQueue(this.curPauseBGMOnFocusTicket);
                this.curPauseBGMOnFocusTicket = null;
            }

            if (this.m_FocusOnlyInteractable)
                this.CanvasGroup.interactable = false;
            OnLostFocused();
        }
        public void OnFocusedCallback()
        {
            if (this.m_PauseBGMOnFocus)
                this.curPauseBGMOnFocusTicket = SoundManager.PauseBGMFromQueue(this);

            if (this.m_FocusOnlyInteractable)
                this.CanvasGroup.interactable = true;
            OnFocused();
        }
        #endregion

        #region Entry Selectable Function
        public bool TrySelectEntrySelectable()
        {
            if (this == null || this.transform == null) return false;
            if (EventSystem.current == null) return false;

            if (this.m_EntrySelectable == null ||
            this.m_EntrySelectable.isActiveAndEnabled == false ||
            this.m_EntrySelectable.IsInteractable() == false)
                return false;

            this.m_EntrySelectable.Select();
            return true;
        }
        public bool TryDeselectIfChildSelected()
        {
            if (this == null || this.transform == null) return false;
            if (EventSystem.current == null) return false;

            var currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null) return false;
            if (currentSelected.transform.IsChildOf(this.transform) == false) return false;

            EventSystem.current.SetSelectedGameObject(null);
            return true;
        }
        #endregion

        protected virtual void OnStartShown() { }
        protected virtual void OnShown() { }
        protected virtual void OnStartClose() { }
        protected virtual void OnClosed() { }
        protected virtual void OnLostFocused() { }
        protected virtual void OnFocused() { }

        public virtual void OnBackButton() { }

        public IEnumerator UntilShown()
        {
            while (this != null && this.IsShown == false)
                yield return null;
            yield break;
        }
        public IEnumerator UntilStartShown()
        {
            while (this != null && this.isShowing == false)
                yield return null;
            yield break;
        }
        public IEnumerator UntilClosed()
        {
            while (this != null && this.IsClosed == false)
                yield return null;
            yield break;
        }
        public IEnumerator UntilStartClosed()
        {
            while (this != null && this.IsClosing == false)
                yield return null;
            yield break;
        }
    }
}
