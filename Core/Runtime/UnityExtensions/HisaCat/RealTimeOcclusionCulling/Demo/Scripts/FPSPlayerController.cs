using UnityEngine;

namespace HisaCat.RealTimeOcclusionCulling.Demo
{
    public class FPSPlayerController : MonoBehaviour
    {
        [SerializeField] private Camera m_Camera = null;
        [SerializeField] private CharacterController m_CharacterController = null;
        [SerializeField] private float m_MoveSpeed = 5f;
        [SerializeField] private float m_MouseSensitivity = 1f;

        private float verticalRotation = 0f;
        private float horizontalRotation = 0f;

        private const float MaxVerticalRotation = 90f;
        private const float MinVerticalRotation = -90f;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            var moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            var rotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            this.verticalRotation = Mathf.Clamp(this.verticalRotation - rotateInput.y * this.m_MouseSensitivity, MinVerticalRotation, MaxVerticalRotation);
            this.m_Camera.transform.localRotation = Quaternion.Euler(this.verticalRotation, 0, 0);

            this.horizontalRotation += rotateInput.x * this.m_MouseSensitivity;
            this.transform.localRotation = Quaternion.Euler(0, this.horizontalRotation, 0);

            var worldMove = this.transform.TransformDirection(moveInput);
            this.m_CharacterController.Move(worldMove * this.m_MoveSpeed * Time.deltaTime);
        }
    }
}
