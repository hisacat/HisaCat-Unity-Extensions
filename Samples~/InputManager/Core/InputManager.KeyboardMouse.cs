using UnityEngine;
using UnityEngine.InputSystem;

namespace HisaCat.HUE.Inputs
{
    public partial class InputManager : MonoBehaviour
    {
        public static class KeyboardMouse
        {
            #region New Input System Wrapper Methods
            public static Vector2 GetMousePosition()
                => Mouse.current?.position.ReadValue() ?? Vector2.zero;
            public static bool GetKeyDown(Key key)
                => Keyboard.current?[key].wasPressedThisFrame ?? false;
            public static bool GetKeyUp(Key key)
                => Keyboard.current?[key].wasReleasedThisFrame ?? false;
            public static bool GetKey(Key key)
                => Keyboard.current?[key].isPressed ?? false;
            #endregion
        }
    }
}
