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
                public static event InputActionCallbackDelegate OnFirePerformed;

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

                    Instance.defaultInputActions.Player.Fire.performed += OnFirePerformedCallback;
                    static void OnFirePerformedCallback(InputAction.CallbackContext ctx) => OnFirePerformed?.Invoke(ctx);

                    IsInitialized = true;
                }

                public static Vector2 GetMovementInput()
                    => Instance.defaultInputActions.Player.Move.ReadValue<Vector2>();

                public static string GetFireButtonDisplayString()
                    => GetCurrentDeviceBindingDisplayString(Instance.defaultInputActions.Player.Fire);
                public static bool GetFireButtonUp()
                    => Instance.defaultInputActions.Player.Fire.GetButtonUp();
                public static bool GetFireButton()
                    => Instance.defaultInputActions.Player.Fire.GetButton();
                public static bool GetFireButtonDown()
                    => Instance.defaultInputActions.Player.Fire.GetButtonDown();

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
                        OnNavigatePerformed = null;

                        IsInitialized = false;
                    }
                }
#pragma warning restore IDE0051
#endif

                public static event InputActionCallbackDelegate OnNavigatePerformed;
                public static event InputActionCallbackDelegate OnSubmitPerformed;
                public static event InputActionCallbackDelegate OnCancelPerformed;

                public static bool IsInitialized { get; private set; } = false;
                public static void InitializeInternal()
                {
                    if (IsInitialized)
                    {
                        Debug.LogError($"[{nameof(InputManager)}.{nameof(UI)}] Already initialized!");
                        return;
                    }

                    Instance.defaultInputActions.UI.Navigate.performed += OnNavigatePerformedCallback;
                    static void OnNavigatePerformedCallback(InputAction.CallbackContext ctx) => OnNavigatePerformed?.Invoke(ctx);

                    Instance.defaultInputActions.UI.Submit.performed += OnSubmitPerformedCallback;
                    static void OnSubmitPerformedCallback(InputAction.CallbackContext ctx) => OnSubmitPerformed?.Invoke(ctx);

                    Instance.defaultInputActions.UI.Cancel.performed += OnCancelPerformedCallback;
                    static void OnCancelPerformedCallback(InputAction.CallbackContext ctx) => OnCancelPerformed?.Invoke(ctx);

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
