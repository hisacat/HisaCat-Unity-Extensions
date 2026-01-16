using HisaCat.PropertyAttributes;
using HisaCat.UnityExtensions;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace HisaCat.HUE.InputSystemExtensions
{
    [DefaultExecutionOrder(int.MinValue)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class InputSystemUIInputModuleExtend : MonoBehaviour
    {
        [ReadOnly][SerializeField] private InputSystemUIInputModule inputSystemUIInputModule = null;
        private void Awake()
        {
            if (this.ConditionLogWarning(this.inputSystemUIInputModule == null,
            $"[{nameof(InputSystemUIInputModuleExtend)}] {nameof(InputSystemUIInputModule)} is not assigned! it will be assigned automatically.", this))
                this.inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
        }
#endif

        public delegate void MousePointerOverGameObjectDelegate(GameObject newGameObject);
        public event MousePointerOverGameObjectDelegate OnMousePointerOverGameObjectChanged = null;

        public GameObject LastMousePointerOverGameObject { get; private set; } = null;
        private void LateUpdate()
        {
            var currentMousePointerOverGameObject = this.inputSystemUIInputModule.GetMousePointerOverGameObject();
            if (currentMousePointerOverGameObject != this.LastMousePointerOverGameObject)
            {
                this.LastMousePointerOverGameObject = currentMousePointerOverGameObject;
                this.OnMousePointerOverGameObjectChanged?.Invoke(currentMousePointerOverGameObject);
            }
        }
    }
}
