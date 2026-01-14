using HisaCat.HUE.Inputs.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
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
                        IsInitialized = false;
                    }
                }
#pragma warning restore IDE0051
#endif

                public static event InputActionCallbackDelegate OnMovePerformed;
                public static event InputActionCallbackDelegate OnMoveCanceled;
                public static event InputActionCallbackDelegate OnAttackPerformed;
                public static event InputActionCallbackDelegate OnAttackCanceled;

                public static bool IsInitialized { get; private set; } = false;
                public static void InitializeInternal()
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

                public static Vector2 GetMovementInput()
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
                public static Vector2 GetFPSCameraLookInput()
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
                        OnNavigateStarted = null;
                        OnSubmitStarted = null;
                        OnCancelStarted = null;

                        IsInitialized = false;
                    }
                }
#pragma warning restore IDE0051
#endif

                public static event InputActionCallbackDelegate OnNavigateStarted;
                public static event InputActionCallbackDelegate OnSubmitStarted;
                public static event InputActionCallbackDelegate OnCancelStarted;

                public static bool IsInitialized { get; private set; } = false;
                public static void InitializeInternal()
                {
                    if (IsInitialized)
                    {
                        Debug.LogError($"[{nameof(InputManager)}.{nameof(UI)}] Already initialized!");
                        return;
                    }

                    Instance.defaultInputActions.UI.Navigate.started += OnNavigatePerformedCallback;
                    static void OnNavigatePerformedCallback(InputAction.CallbackContext ctx) => OnNavigateStarted?.Invoke(ctx);

                    Instance.defaultInputActions.UI.Submit.started += OnSubmitPerformedCallback;
                    static void OnSubmitPerformedCallback(InputAction.CallbackContext ctx) => OnSubmitStarted?.Invoke(ctx);

                    Instance.defaultInputActions.UI.Cancel.started += OnCancelPerformedCallback;
                    static void OnCancelPerformedCallback(InputAction.CallbackContext ctx) => OnCancelStarted?.Invoke(ctx);

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
