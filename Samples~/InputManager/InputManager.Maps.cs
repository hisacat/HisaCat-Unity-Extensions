using HisaCat.HUE.Inputs.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        private static void RegisterInputActionCallbacks(InputAction action,
            (System.Action<InputAction.CallbackContext> onStarted,
            System.Action<InputAction.CallbackContext> onPerformed,
            System.Action<InputAction.CallbackContext> onCanceled) delegates)
        {
            if (delegates.onStarted != null) action.started += delegates.onStarted;
            if (delegates.onPerformed != null) action.performed += delegates.onPerformed;
            if (delegates.onCanceled != null) action.canceled += delegates.onCanceled;
        }

        private static void ClearAndDisposeInputAction(ref InputAction action)
        {
            if (action == null) return;
            action.Dispose();
            action = null;
        }

        public static class Maps
        {
            public static class Player
            {
#if UNITY_EDITOR
#pragma warning disable IDE0051
                [UnityEditor.InitializeOnEnterPlayMode]
                private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
                {
                    if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                    {
                        (OnMovePerformed, OnMoveCanceled) = (null, null);
                        (OnAttackPerformed, OnAttackCanceled) = (null, null);

                        IsInitialized = false;
                    }
                }
#pragma warning restore IDE0051
#endif

                public static event InputActionCallbackDelegate OnMovePerformed = null, OnMoveCanceled = null;
                public static event InputActionCallbackDelegate OnAttackPerformed = null, OnAttackCanceled = null;

                public static bool IsInitialized { get; private set; } = false;
                internal static void InitializeInternal()
                {
                    if (IsInitialized)
                    {
                        Debug.LogError($"[{nameof(InputManager)}.{nameof(Player)}] Already initialized!");
                        return;
                    }

                    Instance.defaultInputActions.Player.Move.performed += OnMovePerformedCallback;
                    static void OnMovePerformedCallback(InputAction.CallbackContext ctx) => OnMovePerformed?.Invoke(ctx);
                    Instance.defaultInputActions.Player.Move.canceled += OnMoveCanceledCallback;
                    static void OnMoveCanceledCallback(InputAction.CallbackContext ctx) => OnMoveCanceled?.Invoke(ctx);

                    Instance.defaultInputActions.Player.Attack.performed += OnAttackPerformedCallback;
                    static void OnAttackPerformedCallback(InputAction.CallbackContext ctx) => OnAttackPerformed?.Invoke(ctx);
                    Instance.defaultInputActions.Player.Attack.canceled += OnAttackCanceledCallback;
                    static void OnAttackCanceledCallback(InputAction.CallbackContext ctx) => OnAttackCanceled?.Invoke(ctx);

                    IsInitialized = true;
                }

                public static Vector2 GetMoveInput()
                    => Instance.defaultInputActions.Player.Move.ReadValue<Vector2>();

                public static string GetAttackButtonDisplayString()
                    => GetCurrentDeviceBindingDisplayString(Instance.defaultInputActions.Player.Attack);
                public static bool GetAttackButtonUp()
                    => Instance.defaultInputActions.Player.Attack.GetButtonUp();
                public static bool GetAttackButton()
                    => Instance.defaultInputActions.Player.Attack.GetButton();
                public static bool GetAttackButtonDown()
                    => Instance.defaultInputActions.Player.Attack.GetButtonDown();

                /// <summary>
                /// This represents the horizontal and vertical values of pixels that need to move per inch for each frame.
                /// </summary>
                public static Vector2 GetLookInput()
                    => Instance.defaultInputActions.Player.Look.ReadValue<Vector2>();
            }

            public static class UI
            {
#if UNITY_EDITOR
#pragma warning disable IDE0051
                [UnityEditor.InitializeOnEnterPlayMode]
                private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
                {
                    if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
                    {
                        (OnNavigatePerformed, OnNavigateCanceled) = (null, null);
                        (OnSubmitPerformed, OnSubmitCanceled) = (null, null);
                        (OnCancelPerformed, OnCancelCanceled) = (null, null);

                        IsInitialized = false;
                    }
                }
#pragma warning restore IDE0051
#endif

                public static event InputActionCallbackDelegate OnNavigatePerformed = null, OnNavigateCanceled = null;
                public static event InputActionCallbackDelegate OnSubmitPerformed = null, OnSubmitCanceled = null;
                public static event InputActionCallbackDelegate OnCancelPerformed = null, OnCancelCanceled = null;

                public static bool IsInitialized { get; private set; } = false;
                internal static void InitializeInternal()
                {
                    if (IsInitialized)
                    {
                        Debug.LogError($"[{nameof(InputManager)}.{nameof(UI)}] Already initialized!");
                        return;
                    }

                    RegisterInputActionCallbacks(Instance.defaultInputActions.UI.Navigate,
                        (onStarted: null, onPerformed: OnNavigatePerformedCallback, onCanceled: OnNavigateCanceledCallback));
                    static void OnNavigatePerformedCallback(InputAction.CallbackContext ctx) => OnNavigatePerformed?.Invoke(ctx);
                    static void OnNavigateCanceledCallback(InputAction.CallbackContext ctx) => OnNavigateCanceled?.Invoke(ctx);

                    RegisterInputActionCallbacks(Instance.defaultInputActions.UI.Submit,
                        (onStarted: null, onPerformed: OnSubmitPerformedCallback, onCanceled: OnSubmitCanceledCallback));
                    static void OnSubmitPerformedCallback(InputAction.CallbackContext ctx) => OnSubmitPerformed?.Invoke(ctx);
                    static void OnSubmitCanceledCallback(InputAction.CallbackContext ctx) => OnSubmitCanceled?.Invoke(ctx);

                    RegisterInputActionCallbacks(Instance.defaultInputActions.UI.Cancel,
                        (onStarted: null, onPerformed: OnCancelPerformedCallback, onCanceled: OnCancelCanceledCallback));
                    static void OnCancelPerformedCallback(InputAction.CallbackContext ctx) => OnCancelPerformed?.Invoke(ctx);
                    static void OnCancelCanceledCallback(InputAction.CallbackContext ctx) => OnCancelCanceled?.Invoke(ctx);

                    IsInitialized = true;
                }

                public static bool GetSubmitButtonDown()
                    => Instance.defaultInputActions.UI.Submit.GetButtonDown();
                public static bool GetSubmitButton()
                    => Instance.defaultInputActions.UI.Submit.GetButton();
                public static bool GetSubmitButtonUp()
                    => Instance.defaultInputActions.UI.Submit.GetButtonUp();

                public static bool GetCancelButtonDown()
                    => Instance.defaultInputActions.UI.Cancel.GetButtonDown();
                public static bool GetCancelButton()
                    => Instance.defaultInputActions.UI.Cancel.GetButton();
                public static bool GetCancelButtonUp()
                    => Instance.defaultInputActions.UI.Cancel.GetButtonUp();
            }
        }
    }
}
