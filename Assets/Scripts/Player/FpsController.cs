using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PortalGame.Player {
    /// <summary>
    /// Controlador de primeira pessoa do player
    /// </summary>
    public class FpsController : MonoBehaviour {

        [Header("Configuracoes")]
        [SerializeField]
        private float mouseSensitivity = 1;

        [SerializeField]
        private float moveSpeed = 5;

        [SerializeField]
        private float jumpForce = 5;

        [SerializeField]
        private bool lockCamera = false;


        [Header("Referencias")]
        [SerializeField]
        private Transform head;

        [SerializeField]
        private Transform feet;

        private Rigidbody rb;
        private Directions currentDirection = Directions.None;
        private bool jumpPressed = false;
        private bool isGrounded = false;

        private void Start() {
            rb = GetComponent<Rigidbody>();
        }

        private void OnEnable() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update() {
            if (lockCamera) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // ler inputs
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // rotaciona player
            transform.Rotate(Vector3.up, mouseX);

            // rotaciona cabeca
            head.Rotate(Vector3.right, -mouseY);

            float dot = Vector3.Dot(head.forward, Vector3.up);
            if (Quaternion.Angle(Quaternion.identity, head.localRotation) > 90) {
                if (dot > 0) {
                    head.localRotation = Quaternion.Euler(-90, 0, 0);
                } else {
                    head.localRotation = Quaternion.Euler(90, 0, 0);
                }
            }

            // ler input de movimento
            currentDirection = Directions.None;
            if (Input.GetKey(KeyCode.W)) {
                currentDirection |= Directions.Forward;
            }
            if (Input.GetKey(KeyCode.S)) {
                currentDirection |= Directions.Backward;
            }
            if (Input.GetKey(KeyCode.A)) {
                currentDirection |= Directions.Left;
            }
            if (Input.GetKey(KeyCode.D)) {
                currentDirection |= Directions.Right;
            }

            // verificar se esta no chao
            isGrounded = Physics.Raycast(feet.position, Vector3.down, 0.1f);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
                jumpPressed = true;
                Debug.Log("Jump pressed");
            }
        }

        private void FixedUpdate() {
            // mover player
            Vector3 direction = Vector3.zero;
            if ((currentDirection & Directions.Forward) != 0) {
                direction += Vector3.forward;
            }
            if ((currentDirection & Directions.Backward) != 0) {
                direction -= Vector3.forward;
            }
            if ((currentDirection & Directions.Left) != 0) {
                direction -= Vector3.right;
            }
            if ((currentDirection & Directions.Right) != 0) {
                direction += Vector3.right;
            }
            direction.Normalize();
            direction = transform.TransformDirection(direction);
            rb.AddForce(direction * moveSpeed,ForceMode.VelocityChange);
            rb.angularVelocity = Vector3.zero;

            if (jumpPressed) {
                Debug.Log("applying jmp force");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpPressed = false;
            }
        }

        [Flags]
        private enum Directions {
            None = 0,
            Left = 1,
            Right = 2,
            Forward = 4,
            Backward = 8
        }
    }
}
