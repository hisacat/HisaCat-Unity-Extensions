using HisaCat.HUE.Inputs.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public static class KeyboardMouse
        {
            public static Vector2 GetMousePosition()
            {
                var mouse = UnityEngine.InputSystem.Mouse.current;
                return mouse == null ? Vector2.zero : mouse.position.ReadValue();
            }
        }
    }
}
