using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public static class KeyboardMouse
        {
#if UNITY_EDITOR
#pragma warning disable IDE0051
            [UnityEditor.InitializeOnEnterPlayMode]
            private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
            {
                if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                {
                    ClearInputAction(ref LeftMouseButtonAction);
                    ClearInputAction(ref RightMouseButtonAction);
                    ClearInputAction(ref MiddleMouseButtonAction);
                    ClearInputAction(ref BackMouseButtonAction);
                    ClearInputAction(ref ForwardMouseButtonAction);
                    ClearInputAction(ref MousePositionAction);
                    ClearInputAction(ref MouseDeltaAction);

                    static void ClearInputAction(ref InputAction action)
                    {
                        if (action == null) return;
                        action.Dispose();
                        action = null;
                    }

                    (OnLeftMouseButtonPerformed, OnLeftMouseButtonCanceled) = (null, null);
                    (OnRightMouseButtonPerformed, OnRightMouseButtonCanceled) = (null, null);
                    (OnMiddleMouseButtonPerformed, OnMiddleMouseButtonCanceled) = (null, null);
                    (OnBackMouseButtonPerformed, OnBackMouseButtonCanceled) = (null, null);
                    (OnForwardMouseButtonPerformed, OnForwardMouseButtonCanceled) = (null, null);
                    (OnMousePositionPerformed, OnMousePositionCanceled) = (null, null);
                    (OnMouseDeltaPerformed, OnMouseDeltaCanceled) = (null, null);

                    IsInitialized = false;
                }
            }
#pragma warning restore IDE0051
#endif

            public static bool IsInitialized { get; private set; } = false;
            internal static void InitializeInternal()
            {
                if (IsInitialized)
                {
                    Debug.LogError($"[{nameof(InputManager)}.{nameof(KeyboardMouse)}] Already initialized!");
                    return;
                }

                CreateAndEnableMouseInputActions();
                DelegateMouseActionDelegates();

                IsInitialized = true;
            }

            #region New Input System Wrapper Methods
            public static InputAction LeftMouseButtonAction = null;
            public static event InputActionCallbackDelegate OnLeftMouseButtonPerformed = null, OnLeftMouseButtonCanceled = null;
            public static InputAction RightMouseButtonAction = null;
            public static event InputActionCallbackDelegate OnRightMouseButtonPerformed = null, OnRightMouseButtonCanceled = null;
            public static InputAction MiddleMouseButtonAction = null;
            public static event InputActionCallbackDelegate OnMiddleMouseButtonPerformed = null, OnMiddleMouseButtonCanceled = null;
            public static InputAction BackMouseButtonAction = null;
            public static event InputActionCallbackDelegate OnBackMouseButtonPerformed = null, OnBackMouseButtonCanceled = null;
            public static InputAction ForwardMouseButtonAction = null;
            public static event InputActionCallbackDelegate OnForwardMouseButtonPerformed = null, OnForwardMouseButtonCanceled = null;
            public static InputAction MousePositionAction = null;
            public static event InputActionCallbackDelegate OnMousePositionPerformed = null, OnMousePositionCanceled = null;
            public static InputAction MouseDeltaAction = null;
            public static event InputActionCallbackDelegate OnMouseDeltaPerformed = null, OnMouseDeltaCanceled = null;

            private static void CreateAndEnableMouseInputActions()
            {
                CreateAndEnableMouseInputAction(ref LeftMouseButtonAction, "LeftMouseButton", "<Mouse>/leftButton");
                CreateAndEnableMouseInputAction(ref RightMouseButtonAction, "RightMouseButton", "<Mouse>/rightButton");
                CreateAndEnableMouseInputAction(ref MiddleMouseButtonAction, "MiddleMouseButton", "<Mouse>/middleButton");
                CreateAndEnableMouseInputAction(ref BackMouseButtonAction, "BackMouseButton", "<Mouse>/backButton");
                CreateAndEnableMouseInputAction(ref ForwardMouseButtonAction, "ForwardMouseButton", "<Mouse>/forwardButton");
                CreateAndEnableMouseInputAction(ref MousePositionAction, "MousePosition", "<Mouse>/position", InputActionType.PassThrough);
                CreateAndEnableMouseInputAction(ref MouseDeltaAction, "MouseDelta", "<Mouse>/delta", InputActionType.PassThrough);

                static void CreateAndEnableMouseInputAction(ref InputAction action, string name, string binding, InputActionType type = InputActionType.Button)
                {
                    action = new(name, type: type, binding: binding);
                    action.Enable();
                }
            }
            private static void DelegateMouseActionDelegates()
            {
                RegisterInputActionCallbacks(LeftMouseButtonAction,
                    (onStarted: null, onPerformed: OnLeftMouseButtonPerformedCallback, onCanceled: OnLeftMouseButtonCanceledCallback));
                static void OnLeftMouseButtonPerformedCallback(InputAction.CallbackContext ctx) => OnLeftMouseButtonPerformed?.Invoke(ctx);
                static void OnLeftMouseButtonCanceledCallback(InputAction.CallbackContext ctx) => OnLeftMouseButtonCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(RightMouseButtonAction,
                    (onStarted: null, onPerformed: OnRightMouseButtonPerformedCallback, onCanceled: OnRightMouseButtonCanceledCallback));
                static void OnRightMouseButtonPerformedCallback(InputAction.CallbackContext ctx) => OnRightMouseButtonPerformed?.Invoke(ctx);
                static void OnRightMouseButtonCanceledCallback(InputAction.CallbackContext ctx) => OnRightMouseButtonCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(MiddleMouseButtonAction,
                    (onStarted: null, onPerformed: OnMiddleMouseButtonPerformedCallback, onCanceled: OnMiddleMouseButtonCanceledCallback));
                static void OnMiddleMouseButtonPerformedCallback(InputAction.CallbackContext ctx) => OnMiddleMouseButtonPerformed?.Invoke(ctx);
                static void OnMiddleMouseButtonCanceledCallback(InputAction.CallbackContext ctx) => OnMiddleMouseButtonCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(BackMouseButtonAction,
                    (onStarted: null, onPerformed: OnBackMouseButtonPerformedCallback, onCanceled: OnBackMouseButtonCanceledCallback));
                static void OnBackMouseButtonPerformedCallback(InputAction.CallbackContext ctx) => OnBackMouseButtonPerformed?.Invoke(ctx);
                static void OnBackMouseButtonCanceledCallback(InputAction.CallbackContext ctx) => OnBackMouseButtonCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(ForwardMouseButtonAction,
                    (onStarted: null, onPerformed: OnForwardMouseButtonPerformedCallback, onCanceled: OnForwardMouseButtonCanceledCallback));
                static void OnForwardMouseButtonPerformedCallback(InputAction.CallbackContext ctx) => OnForwardMouseButtonPerformed?.Invoke(ctx);
                static void OnForwardMouseButtonCanceledCallback(InputAction.CallbackContext ctx) => OnForwardMouseButtonCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(MousePositionAction,
                    (onStarted: null, onPerformed: OnMousePositionPerformedCallback, onCanceled: OnMousePositionCanceledCallback));
                static void OnMousePositionPerformedCallback(InputAction.CallbackContext ctx) => OnMousePositionPerformed?.Invoke(ctx);
                static void OnMousePositionCanceledCallback(InputAction.CallbackContext ctx) => OnMousePositionCanceled?.Invoke(ctx);

                RegisterInputActionCallbacks(MouseDeltaAction,
                    (onStarted: null, onPerformed: OnMouseDeltaPerformedCallback, onCanceled: OnMouseDeltaCanceledCallback));
                static void OnMouseDeltaPerformedCallback(InputAction.CallbackContext ctx) => OnMouseDeltaPerformed?.Invoke(ctx);
                static void OnMouseDeltaCanceledCallback(InputAction.CallbackContext ctx) => OnMouseDeltaCanceled?.Invoke(ctx);
            }

            public static Vector2 GetMousePosition()
                => Mouse.current?.position.ReadValue() ?? Vector2.zero;
            public static Vector2 GetMouseDelta()
                => Mouse.current?.delta.ReadValue() ?? Vector2.zero;
            public static Vector2 GetMouseScroll()
                => Mouse.current?.scroll.ReadValue() ?? Vector2.zero;

            public static bool GetLeftMouseButtonDown()
                => Mouse.current?.leftButton.wasPressedThisFrame ?? false;
            public static bool GetLeftMouseButtonUp()
                => Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
            public static bool GetLeftMouseButton()
                => Mouse.current?.leftButton.isPressed ?? false;
            public static bool GetRightMouseButtonDown()
                => Mouse.current?.rightButton.wasPressedThisFrame ?? false;
            public static bool GetRightMouseButtonUp()
                => Mouse.current?.rightButton.wasReleasedThisFrame ?? false;
            public static bool GetRightMouseButton()
                => Mouse.current?.rightButton.isPressed ?? false;
            public static bool GetMiddleMouseButtonDown()
                => Mouse.current?.middleButton.wasPressedThisFrame ?? false;
            public static bool GetMiddleMouseButtonUp()
                => Mouse.current?.middleButton.wasReleasedThisFrame ?? false;
            public static bool GetMiddleMouseButton()
                => Mouse.current?.middleButton.isPressed ?? false;
            public static bool GetBackMouseButtonDown()
                => Mouse.current?.backButton.wasPressedThisFrame ?? false;
            public static bool GetBackMouseButtonUp()
                => Mouse.current?.backButton.wasReleasedThisFrame ?? false;
            public static bool GetBackMouseButton()
                => Mouse.current?.backButton.isPressed ?? false;
            public static bool GetForwardMouseButtonDown()
                => Mouse.current?.forwardButton.wasPressedThisFrame ?? false;
            public static bool GetForwardMouseButtonUp()
                => Mouse.current?.forwardButton.wasReleasedThisFrame ?? false;
            public static bool GetForwardMouseButton()
                => Mouse.current?.forwardButton.isPressed ?? false;

            public static bool GetKeyDown(Key key)
                => Keyboard.current?[key].wasPressedThisFrame ?? false;
            public static bool GetKeyUp(Key key)
                => Keyboard.current?[key].wasReleasedThisFrame ?? false;
            public static bool GetKey(Key key)
                => Keyboard.current?[key].isPressed ?? false;
            #endregion New Input System Wrapper Methods

            #region KeyCode <-> Key Conversion Methods
            public static KeyCode ToKeyCode(Key key)
            {
                switch (key)
                {
                    case Key.None: return KeyCode.None;
                    case Key.Space: return KeyCode.Space;
                    case Key.Enter: return KeyCode.Return;
                    case Key.Tab: return KeyCode.Tab;
                    case Key.Backquote: return KeyCode.BackQuote;
                    case Key.Quote: return KeyCode.Quote;
                    case Key.Semicolon: return KeyCode.Semicolon;
                    case Key.Comma: return KeyCode.Comma;
                    case Key.Period: return KeyCode.Period;
                    case Key.Slash: return KeyCode.Slash;
                    case Key.Backslash: return KeyCode.Backslash;
                    case Key.LeftBracket: return KeyCode.LeftBracket;
                    case Key.RightBracket: return KeyCode.RightBracket;
                    case Key.Minus: return KeyCode.Minus;
                    case Key.Equals: return KeyCode.Equals;
                    case Key.A: return KeyCode.A;
                    case Key.B: return KeyCode.B;
                    case Key.C: return KeyCode.C;
                    case Key.D: return KeyCode.D;
                    case Key.E: return KeyCode.E;
                    case Key.F: return KeyCode.F;
                    case Key.G: return KeyCode.G;
                    case Key.H: return KeyCode.H;
                    case Key.I: return KeyCode.I;
                    case Key.J: return KeyCode.J;
                    case Key.K: return KeyCode.K;
                    case Key.L: return KeyCode.L;
                    case Key.M: return KeyCode.M;
                    case Key.N: return KeyCode.N;
                    case Key.O: return KeyCode.O;
                    case Key.P: return KeyCode.P;
                    case Key.Q: return KeyCode.Q;
                    case Key.R: return KeyCode.R;
                    case Key.S: return KeyCode.S;
                    case Key.T: return KeyCode.T;
                    case Key.U: return KeyCode.U;
                    case Key.V: return KeyCode.V;
                    case Key.W: return KeyCode.W;
                    case Key.X: return KeyCode.X;
                    case Key.Y: return KeyCode.Y;
                    case Key.Z: return KeyCode.Z;
                    case Key.Digit1: return KeyCode.Alpha1;
                    case Key.Digit2: return KeyCode.Alpha2;
                    case Key.Digit3: return KeyCode.Alpha3;
                    case Key.Digit4: return KeyCode.Alpha4;
                    case Key.Digit5: return KeyCode.Alpha5;
                    case Key.Digit6: return KeyCode.Alpha6;
                    case Key.Digit7: return KeyCode.Alpha7;
                    case Key.Digit8: return KeyCode.Alpha8;
                    case Key.Digit9: return KeyCode.Alpha9;
                    case Key.Digit0: return KeyCode.Alpha0;
                    case Key.LeftShift: return KeyCode.LeftShift;
                    case Key.RightShift: return KeyCode.RightShift;
                    case Key.LeftAlt: return KeyCode.LeftAlt;
                    case Key.RightAlt: return KeyCode.RightAlt;
                    // case Key.AltGr: // Same as Key.RightAlt
                    case Key.LeftCtrl: return KeyCode.LeftControl;
                    case Key.RightCtrl: return KeyCode.RightControl;
                    case Key.LeftWindows: return KeyCode.LeftWindows;
                    // case Key.LeftMeta: // Same as Key.LeftWindows
                    // case Key.LeftApple: // Same as Key.LeftWindows
                    // case Key.LeftCommand: // Same as Key.LeftWindows
                    case Key.RightWindows: return KeyCode.RightWindows;
                    // case Key.RightMeta: // Same as Key.RightWindows
                    // case Key.RightApple: // Same as Key.RightWindows
                    // case Key.RightCommand: // Same as Key.RightWindows
                    case Key.ContextMenu: return KeyCode.Menu;
                    case Key.Escape: return KeyCode.Escape;
                    case Key.LeftArrow: return KeyCode.LeftArrow;
                    case Key.RightArrow: return KeyCode.RightArrow;
                    case Key.UpArrow: return KeyCode.UpArrow;
                    case Key.DownArrow: return KeyCode.DownArrow;
                    case Key.Backspace: return KeyCode.Backspace;
                    case Key.PageDown: return KeyCode.PageDown;
                    case Key.PageUp: return KeyCode.PageUp;
                    case Key.Home: return KeyCode.Home;
                    case Key.End: return KeyCode.End;
                    case Key.Insert: return KeyCode.Insert;
                    case Key.Delete: return KeyCode.Delete;
                    case Key.CapsLock: return KeyCode.CapsLock;
                    case Key.NumLock: return KeyCode.Numlock;
                    case Key.PrintScreen: return KeyCode.Print;
                    case Key.ScrollLock: return KeyCode.ScrollLock;
                    case Key.Pause: return KeyCode.Pause;
                    case Key.NumpadEnter: return KeyCode.KeypadEnter;
                    case Key.NumpadDivide: return KeyCode.KeypadDivide;
                    case Key.NumpadMultiply: return KeyCode.KeypadMultiply;
                    case Key.NumpadPlus: return KeyCode.KeypadPlus;
                    case Key.NumpadMinus: return KeyCode.KeypadMinus;
                    case Key.NumpadPeriod: return KeyCode.KeypadPeriod;
                    case Key.NumpadEquals: return KeyCode.KeypadEquals;
                    case Key.Numpad0: return KeyCode.Keypad0;
                    case Key.Numpad1: return KeyCode.Keypad1;
                    case Key.Numpad2: return KeyCode.Keypad2;
                    case Key.Numpad3: return KeyCode.Keypad3;
                    case Key.Numpad4: return KeyCode.Keypad4;
                    case Key.Numpad5: return KeyCode.Keypad5;
                    case Key.Numpad6: return KeyCode.Keypad6;
                    case Key.Numpad7: return KeyCode.Keypad7;
                    case Key.Numpad8: return KeyCode.Keypad8;
                    case Key.Numpad9: return KeyCode.Keypad9;
                    case Key.F1: return KeyCode.F1;
                    case Key.F2: return KeyCode.F2;
                    case Key.F3: return KeyCode.F3;
                    case Key.F4: return KeyCode.F4;
                    case Key.F5: return KeyCode.F5;
                    case Key.F6: return KeyCode.F6;
                    case Key.F7: return KeyCode.F7;
                    case Key.F8: return KeyCode.F8;
                    case Key.F9: return KeyCode.F9;
                    case Key.F10: return KeyCode.F10;
                    case Key.F11: return KeyCode.F11;
                    case Key.F12: return KeyCode.F12;
                    // case Key.OEM1: // Not exists
                    // case Key.OEM2: // Not exists
                    // case Key.OEM3: // Not exists
                    // case Key.OEM4: // Not exists
                    // case Key.OEM5: // Not exists
                    #pragma warning disable CS0618
                    case Key.IMESelected: return KeyCode.None;
                    #pragma warning restore CS0618
                    case Key.F13: return KeyCode.F13;
                    case Key.F14: return KeyCode.F14;
                    case Key.F15: return KeyCode.F15;
                    case Key.F16: return KeyCode.F16;
                    case Key.F17: return KeyCode.F17;
                    case Key.F18: return KeyCode.F18;
                    case Key.F19: return KeyCode.F19;
                    case Key.F20: return KeyCode.F20;
                    case Key.F21: return KeyCode.F21;
                    case Key.F22: return KeyCode.F22;
                    case Key.F23: return KeyCode.F23;
                    case Key.F24: return KeyCode.F24;
                    // case Key.MediaPlayPause: // Not exists
                    // case Key.MediaRewind: // Not exists
                    // case Key.MediaForward: // Not exists
                    default: Debug.LogError($"Unknown Key: {key}"); return KeyCode.None;
                }
            }
            public static Key ToKey(KeyCode keyCode)
            {
                switch (keyCode)
                {
                    // case KeyCode.Greater: // Not exists
                    // case KeyCode.Question: // Not exists
                    // case KeyCode.At: // Not exists
                    case KeyCode.LeftBracket: return Key.LeftBracket;
                    case KeyCode.Backslash: return Key.Backslash;
                    case KeyCode.RightBracket: return Key.RightBracket;
                    case KeyCode.Caret: // Not exists
                    // case KeyCode.Underscore: // Not exists
                    case KeyCode.BackQuote: return Key.Backquote;
                    case KeyCode.A: return Key.A;
                    case KeyCode.B: return Key.B;
                    case KeyCode.C: return Key.C;
                    case KeyCode.D: return Key.D;
                    case KeyCode.E: return Key.E;
                    case KeyCode.F: return Key.F;
                    case KeyCode.G: return Key.G;
                    case KeyCode.H: return Key.H;
                    case KeyCode.I: return Key.I;
                    case KeyCode.J: return Key.J;
                    case KeyCode.K: return Key.K;
                    case KeyCode.L: return Key.L;
                    case KeyCode.M: return Key.M;
                    case KeyCode.N: return Key.N;
                    case KeyCode.O: return Key.O;
                    case KeyCode.P: return Key.P;
                    case KeyCode.Q: return Key.Q;
                    case KeyCode.R: return Key.R;
                    case KeyCode.S: return Key.S;
                    case KeyCode.T: return Key.T;
                    case KeyCode.U: return Key.U;
                    case KeyCode.V: return Key.V;
                    case KeyCode.W: return Key.W;
                    case KeyCode.X: return Key.X;
                    case KeyCode.Y: return Key.Y;
                    case KeyCode.Z: return Key.Z;
                    // case KeyCode.LeftCurlyBracket: // Not exists
                    // case KeyCode.Pipe: // Not exists
                    // case KeyCode.RightCurlyBracket: // Not exists
                    // case KeyCode.Tilde: // Not exists
                    case KeyCode.Numlock: return Key.NumLock;
                    case KeyCode.CapsLock: return Key.CapsLock;
                    case KeyCode.ScrollLock: return Key.ScrollLock;
                    case KeyCode.RightShift: return Key.RightShift;
                    case KeyCode.LeftShift: return Key.LeftShift;
                    case KeyCode.RightControl: return Key.RightCtrl;
                    case KeyCode.LeftControl: return Key.LeftCtrl;
                    case KeyCode.RightAlt: return Key.RightAlt;
                    case KeyCode.LeftAlt: return Key.LeftAlt;
                    case KeyCode.LeftCommand: return Key.LeftCommand;
                    // case KeyCode.LeftMeta: return Key.LeftMeta; // Same as KeyCode.LeftCommand
                    // case KeyCode.LeftApple: return Key.LeftApple; // Same as KeyCode.LeftCommand
                    // case KeyCode.LeftWindows: return Key.LeftWindows; // Same as KeyCode.LeftCommand
                    case KeyCode.RightCommand: return Key.RightCommand;
                    // case KeyCode.RightMeta: return Key.RightMeta; // Same as KeyCode.RightCommand
                    // case KeyCode.RightApple: return Key.RightApple; // Same as KeyCode.RightCommand
                    // case KeyCode.RightWindows: return Key.RightWindows; // Same as KeyCode.RightCommand
                    case KeyCode.AltGr: return Key.AltGr;
                    // case KeyCode.Help: return Key.Help; // Not exists
                    case KeyCode.Print: return Key.PrintScreen;
                    // case KeyCode.SysReq: // Not exists
                    // case KeyCode.Break: // Not exists
                    case KeyCode.Menu: return Key.ContextMenu;
                    // case KeyCode.WheelUp: // Not exists
                    // case KeyCode.WheelDown: // Not exists
                    case KeyCode.F16: return Key.F16;
                    case KeyCode.F17: return Key.F17;
                    case KeyCode.F18: return Key.F18;
                    case KeyCode.F19: return Key.F19;
                    case KeyCode.F20: return Key.F20;
                    case KeyCode.F21: return Key.F21;
                    case KeyCode.F22: return Key.F22;
                    case KeyCode.F23: return Key.F23;
                    case KeyCode.F24: return Key.F24;
                    // case KeyCode.Mouse?: // Not exists
                    // case KeyCode.Joystick?Button?: // Not exists
                    default: Debug.LogError($"Unknown KeyCode: {keyCode}"); return Key.None;
                }
            }
            #endregion KeyCode <-> Key Conversion Methods
        }
    }
}
