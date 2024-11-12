using UnityEngine;

namespace PortalGame
{
    /// <summary>
    /// Move a camera do player com o mouse
    /// </summary>
    public class MouseLook : MonoBehaviour {
        [SerializeField, Tooltip("Sensibilidade do mouse")]
        private float mouseSensitivity = 100f;
        [SerializeField, Tooltip("Referencia ao corpo do jogador")]
        private Transform playerBody;

        private float xRotation = 0f;

#pragma warning disable S2325
        private void Start() {
            Cursor.lockState = CursorLockMode.Locked;
        }
#pragma warning restore S2325

        private void Update() {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
