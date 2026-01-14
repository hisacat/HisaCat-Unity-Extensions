using HisaCat.HUE.Inputs.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        internal static void RegisterInputActionCallbacks(InputAction action,
            (InputActionCallbackDelegate started, InputActionCallbackDelegate performed, InputActionCallbackDelegate canceled) delegates)
        {
            action.started += StartedCallback; action.performed += PerformedCallback; action.canceled += CanceledCallback;
            void StartedCallback(InputAction.CallbackContext ctx) => delegates.started?.Invoke(ctx);
            void PerformedCallback(InputAction.CallbackContext ctx) => delegates.performed?.Invoke(ctx);
            void CanceledCallback(InputAction.CallbackContext ctx) => delegates.canceled?.Invoke(ctx);
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

                    RegisterInputActionCallbacks(Instance.defaultInputActions.Player.Move,
                        (started: null, performed: OnMovePerformed, canceled: OnMoveCanceled));
                    RegisterInputActionCallbacks(Instance.defaultInputActions.Player.Attack,
                        (started: null, performed: OnAttackPerformed, canceled: OnAttackCanceled));

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
                        (started: null, performed: OnNavigatePerformed, canceled: OnNavigateCanceled));
                    RegisterInputActionCallbacks(Instance.defaultInputActions.UI.Submit,
                        (started: null, performed: OnSubmitPerformed, canceled: OnSubmitCanceled));
                    RegisterInputActionCallbacks(Instance.defaultInputActions.UI.Cancel,
                        (started: null, performed: OnCancelPerformed, canceled: OnCancelCanceled));

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
