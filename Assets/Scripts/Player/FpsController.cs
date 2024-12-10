using UnityEngine;

namespace PortalGame.Player
{
    public class FpsController : MonoBehaviour
    {
        private PlayerInput playerInput;

        private Rigidbody rb;

        [Header("Movimento")]
        [SerializeField]
        private float jumpForce = 5;
        [SerializeField]
        private float moveSpeed = 5;
        [SerializeField]
        private float mouseSensitivity = 5;
        [SerializeField]
        private float groundFriction = 5;
        [SerializeField]
        private float airFriction = 0.1f;

        [Header("Ground")]
        [SerializeField]
        private LayerMask groundMask;
        [SerializeField]
        private float groundDistance = 0.1f;
        private bool isGrounded = true;

        [Header("Referencias")]
        [SerializeField]
        private Transform groundCheck;
        [SerializeField]
        private Transform head;

        [field: SerializeField]
        public bool BlockMovement { get; set; } = false;

        // inputs
        private Vector2 movementInput;
        private Vector2 lookInput;
        private Quaternion bodyTargetRotation;
        private Quaternion headTargetRotation;

        private void OnEnable()
        {
            playerInput = new();
            playerInput.Enable();

            playerInput.Player.Jump.performed += ctx => Jump();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Cursor.lockState = !BlockMovement ? CursorLockMode.Locked : CursorLockMode.None;

            // atualiza friccao
            rb.linearDamping = isGrounded ? groundFriction : airFriction;

            // verificar se ta no chao
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            Debug.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundDistance, isGrounded ? Color.green : Color.red);

            // desenhar velocide linear
            Debug.DrawLine(transform.position, transform.position + rb.linearVelocity, Color.blue);

            // desenha direcao da cabeca
            Debug.DrawLine(head.position, head.position + head.forward, Color.magenta);

            // movimento
            movementInput = playerInput.Player.Move.ReadValue<Vector2>();
            lookInput = playerInput.Player.Look.ReadValue<Vector2>();

            // calcula novas rotacoes alvo
            bodyTargetRotation = transform.rotation * Quaternion.Euler(0, lookInput.x, 0);
            headTargetRotation = head.localRotation * Quaternion.Euler(lookInput.y, 0, 0);
            
            // clamp a rotacao da cabeca
            var hAngle = Quaternion.Angle(Quaternion.identity, headTargetRotation);
            var vAngle = Quaternion.Angle(Quaternion.Euler(90, 0, 0), headTargetRotation);

            if(hAngle > 90) {
                if(vAngle < 90) {
                    headTargetRotation = Quaternion.Euler(90, 0, 0);
                } else {
                    headTargetRotation = Quaternion.Euler(-90, 0, 0);
                }
            }

            if (BlockMovement) {
                return;
            }
            // rotaciona o corpo
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyTargetRotation, mouseSensitivity * Time.deltaTime * 10);

            // rotaciona a cabeca
            head.localRotation = Quaternion.Slerp(head.localRotation, headTargetRotation, mouseSensitivity * Time.deltaTime * 10);
        }

        private void FixedUpdate() {
            // aplica forca para mover o player
            movementInput.Normalize();
            if (isGrounded && !BlockMovement) {
                rb.AddForce(moveSpeed * movementInput.y * Time.fixedDeltaTime * transform.forward, ForceMode.VelocityChange);
                rb.AddForce(moveSpeed * movementInput.x * Time.fixedDeltaTime * transform.right, ForceMode.VelocityChange);
            }
        }

        private void OnDrawGizmos() {
            var c = isGrounded ? Color.green : Color.red;
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        private void Jump() {
            if (isGrounded && !BlockMovement) {
                rb.AddForce(jumpForce * 10 * Vector3.up, ForceMode.Impulse);    
            }
        }

        private void OnDisable() {
            playerInput.Disable();
        }
    }
}
