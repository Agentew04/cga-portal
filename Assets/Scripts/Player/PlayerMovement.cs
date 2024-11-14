using UnityEngine;

namespace PortalGame.Player {

    /// <summary>
    /// Controla a movimentacao do player
    /// </summary>
    public class PlayerMovement : MonoBehaviour {
        [SerializeField, Tooltip("Referencia ao character controller")]
        private CharacterController controller;
        [SerializeField, Tooltip("Velocidade de movimento enquanto anda")]
        private float speed = 12f;
        [SerializeField, Tooltip("Velocidade de movimento enquanto agacha")]
        private float crouchSpeed = 6f;
        [SerializeField, Tooltip("Gravidade")]
        private float gravity = -9.81f;
        [SerializeField, Tooltip("Altura do pulo")]
        private float jumpHeight = 3f;

        private Vector3 velocity;
        private bool isGrounded;
        private bool isCrouching = false;

        [Space]
        [SerializeField, Tooltip("Posicao dos pes do jogador")] 
        private Transform groundCheck;
        [SerializeField, Tooltip("Distancia maxima dos pes do jogador ate o chao")]
        private float groundDistance = 0.4f;
        [SerializeField, Tooltip("Layers que sao consideradas como chao")]
        private LayerMask groundMask;

        [Space]
        [SerializeField, Tooltip("Altura enquanto fica de pe")]
        private float standHeight = 2f;
        [SerializeField, Tooltip("Altura enquanto agacha")]
        private float crouchHeight = 1f;

        [Space]
        [SerializeField]
        private bool blockMovement = false;

        public bool BlockMovement {
            get => blockMovement;
            set => blockMovement = value;
        }

        private void Update() {
            if (blockMovement) {
                return;
            }

            // Verifica se está no chão
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0) {
                velocity.y = -2f;
            }

            // Movimentação básica WASD
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            float currentSpeed = isCrouching ? crouchSpeed : speed;
            Vector3 move = transform.right * x + transform.forward * z;
            if (move.magnitude > 1) {
                move.Normalize();
            }
            controller.Move(currentSpeed * Time.deltaTime * move);

            // Pulo
            if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching) {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Aplicação de gravidade
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Controle de agachamento
            if (Input.GetKeyDown(KeyCode.LeftControl)) {
                Crouch();
            }
            if (Input.GetKeyUp(KeyCode.LeftControl)) {
                StandUp();
            }
        }

        private void Crouch() {
            controller.height = crouchHeight;
            isCrouching = true;
        }

        private void StandUp() {
            controller.height = standHeight;
            isCrouching = false;
        }
    }
}
