using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable IDE0051
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                Instance = null;
                blockInputTickets = new HashSet<BlockInputTicket>();
            }
        }
#pragma warning restore IDE0051
#endif

        public static InputManager Instance { get; private set; }

        [SerializeField] private EventSystem m_EventSystem;
        public EventSystem EventSystem { get => this.m_EventSystem; }

        [SerializeField] private InputSystemUIInputModule m_InputSystemUIInputModule;
        public InputSystemUIInputModule InputSystemUIInputModule { get => this.m_InputSystemUIInputModule; }

        private System.IDisposable onAnyButtonPressHandler = null;
        private string DefaultControlScheme => this.defaultInputActions.KeyboardMouseScheme.name;
        public InputDevice LastInputDevice { get; private set; } = null;
        public string LastControlScheme { get; private set; } = null;

        private void Awake()
        {
            if (Instance != null)
            {

                ManagedDebug.LogError($"[{nameof(InputManager)}] {nameof(Awake)}: Instance already exists!");
                Destroy(this.gameObject);
                return;
            }

            // Initialize input action
            this.CreateInputActions();

            // Initialize & Caching last control scheme
            this.LastControlScheme = this.DefaultControlScheme;
            this.onAnyButtonPressHandler = InputSystem.onAnyButtonPress.Call((control) =>
            {
                var lastDevice = control.device;
                if (lastDevice == null)
                {
                    this.LastControlScheme = this.DefaultControlScheme;
                    return;
                }
                this.LastInputDevice = lastDevice;

                switch (this.LastInputDevice)
                {
                    case Keyboard _:
                    case Mouse _:
                        this.LastControlScheme = this.defaultInputActions.KeyboardMouseScheme.name;
                        break;
                    case Gamepad _:
                        this.LastControlScheme = this.defaultInputActions.GamepadScheme.name;
                        break;
                    case Touchscreen _:
                        this.LastControlScheme = this.defaultInputActions.TouchScheme.name;
                        break;
                    case Joystick _:
                        this.LastControlScheme = this.defaultInputActions.JoystickScheme.name;
                        break;
                    case XRController _:
                        this.LastControlScheme = this.defaultInputActions.XRScheme.name;
                        break;
                    default:
                        this.LastControlScheme = this.DefaultControlScheme;
                        break;
                }
            });

            // Enable action maps
            this.defaultInputActions.Player.Enable();
            this.defaultInputActions.UI.Enable();

            Instance = this;

            // Initialize Map wrapper class.
            Maps.Player.InitializeInternal();
            Maps.UI.InitializeInternal();

            // [NOTE] Example for uses using InputAction's interactions.
            // this.defaultInputActions.Player.Interact.started += OnPlayerInteractStarted;
            // void OnPlayerInteractStarted(InputAction.CallbackContext context)
            // {
            //     switch (context.interaction)
            //     {
            //         case HoldInteraction holdInteraction:
            //             break;
            //         case MultiTapInteraction multiTapInteraction:
            //             break;
            //         case PressInteraction pressInteraction:
            //             break;
            //         case SlowTapInteraction slowTapInteraction:
            //             break;
            //         case TapInteraction tapInteraction:
            //             break;
            //     }
            // }
        }
        private void OnDestroy()
        {
            this.onAnyButtonPressHandler?.Dispose();
            this.onAnyButtonPressHandler = null;

            Instance = null;
        }

        private static bool TryGetBindingIndexByScheme(InputAction action, string scheme, out int index)
        {
            var bindingCount = action.bindings.Count;
            for (int i = 0; i < bindingCount; i++)
            {
                var binding = action.bindings[i];
                if (binding.groups.Contains(scheme))
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
        private static string GetCurrentDeviceBindingDisplayString(InputAction action)
        {
            int index;
            if (TryGetBindingIndexByScheme(action, Instance.LastControlScheme, out index))
                return action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
            else if (TryGetBindingIndexByScheme(action, Instance.DefaultControlScheme, out index))
                return action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontIncludeInteractions);

            return action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }

        public delegate void InputActionCallbackDelegate(InputAction.CallbackContext ctx);
    }
}
