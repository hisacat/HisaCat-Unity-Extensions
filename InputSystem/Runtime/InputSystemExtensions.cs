using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace HisaCat.HUE.InputSystemExtensions
{
    public static class InputSystemUIInputModuleExtensions
    {
        public static GameObject GetPointerOverGameObject(this InputSystemUIInputModule inputModule, int pointerOrTouchId)
        {
            // GetLastRaycastResult uses the device ID for pointer devices like mouse.
            // Reference:
            // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html
            // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/api/UnityEngine.InputSystem.InputDevice.html#UnityEngine_InputSystem_InputDevice_deviceId
            var raycastResult = inputModule.GetLastRaycastResult(pointerOrTouchId);
            return raycastResult.isValid ? raycastResult.gameObject : null;
        }
        public static GameObject GetMousePointerOverGameObject(this InputSystemUIInputModule inputModule)
            => inputModule.GetPointerOverGameObject(Mouse.current.deviceId);
    }
}
