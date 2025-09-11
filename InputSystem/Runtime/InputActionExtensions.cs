
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs.Extensions
{
    public static class InputActionExtensions
    {
        public static bool GetButtonDown(this InputAction inputAction)
            => inputAction.WasPressedThisFrame();
        public static bool GetButton(this InputAction inputAction)
            => inputAction.ReadValue<float>() > 0f;
        public static bool GetButtonUp(this InputAction inputAction)
            => inputAction.WasReleasedThisFrame();
    }
}