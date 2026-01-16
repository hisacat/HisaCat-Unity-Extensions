using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HisaCat.HUE.InputSystemExtensions
{
    public static class InputSystemUIInputModuleExtensions
    {
        public static GameObject GetPointerOverGameObject(this InputSystemUIInputModule inputModule, int pointerOrTouchId)
        {
            var raycastResult = inputModule.GetLastRaycastResult(pointerOrTouchId);
            return raycastResult.isValid ? raycastResult.gameObject : null;
        }
        public static GameObject GetMousePointerOverGameObject(this InputSystemUIInputModule inputModule)
            => inputModule.GetPointerOverGameObject(PointerInputModule.kMouseLeftId);
    }
}
