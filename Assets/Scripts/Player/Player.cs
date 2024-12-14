using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PortalGame.World;

namespace PortalGame.Player {

    /// <summary>
    /// Classe 
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Player : MonoBehaviour {

        [Header("Referencias")]
        [SerializeField]
        private Gun gun;

        [SerializeField]
        private PauseMenu pauseMenu;

        [SerializeField]
        private GameObject portalPrefab;

        [SerializeField, Tooltip("Usado para mutar o audio do jogador")]
        private AudioListener audioListener;

        [SerializeField, Tooltip("Usado para pegar uma textura da camera")]
        private new Camera camera;
        public Camera Camera => camera;

        [field: SerializeField]
        public Portal RedPortal { get; set; } = null;

        [field: SerializeField]
        public Portal BluePortal { get; set; } = null;

        private FpsController fpsController;

        private bool isLocked = false;
        private bool isMuted = false;

        [Header("Vida")]
        [SerializeField]
        private float maxHealth = 100;
        [SerializeField]
        private float currentHealth = 100;
        [SerializeField, Tooltip("Depois dessa quantidade de segundos de um combate, o jogador regenera vida")]
        private float combatTimer = 10;
        [SerializeField, Tooltip("Quantos pontos de vida por segundo regenera a vida fora de combate")]
        private float regenRate = 10;

        private float lastCombatTime = 0;

        [Header("Pegador")]
        [SerializeField, Tooltip("Distancia maxima que pode pegar objetos")]
        private float grabDistance = 5;
        [SerializeField]
        private Transform grabPoint;
        [SerializeField]
        private GameObject grabbed;
        private GameObject grabbedLastParent;
        private float lastAngularDamping;
        private float lastLinearDamping;
        [SerializeField]
        private LayerMask grabbableLayer;

        private void Start() {
            RenderPipelineManager.beginCameraRendering += RenderPortals;
            fpsController = FindAnyObjectByType<FpsController>();
            currentHealth = maxHealth;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) {
                SetLock(!isLocked);
                pauseMenu.TogglePause();
            }

            GrabUpdate();
            UpdateHealth();

            if (isLocked) {
                return;
            }

            // a partir daqui so atualiza quando nao pausado

            if(Input.GetMouseButtonDown(0)){
                Click(PortalType.Blue);
            } else if (Input.GetMouseButtonDown(1)) {
                Click(PortalType.Orange);
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                GrabObject();
            }
        }

        private void RenderPortals(ScriptableRenderContext ctx, Camera cam) {
            if(BluePortal == null || RedPortal == null) {
                return;
            }
            if(cam != Camera.main) {
                return;
            }
            BluePortal.PrePortalRender();
            RedPortal.PrePortalRender();

            BluePortal.Render(ctx);
            RedPortal.Render(ctx);

            BluePortal.PostPortalRender();
            RedPortal.PostPortalRender();
        }

        private void Click(PortalType type) {
            if(fpsController.BlockMovement) {
                return;
            }
            
            Ray ray = new(transform.position, transform.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, 1000.0f);

            if(!isHit) {
                return;
            }

            bool validSurface = hit.collider.gameObject.layer == LayerMask.NameToLayer("Portalable");

            foreach(var turret in TurretManager.Instance.GetTurrets()) {
                turret.GiveHint(hit.point);
            }
            
            Debug.DrawLine(ray.origin, hit.point, Color.red, 5);
            gun.ShootProjectile(type, validSurface, () => {
                if (!validSurface) {
                    return;
                }
                if(type == PortalType.Blue) {
                    // cria o portal
                    if(BluePortal == null) {
                        BluePortal = CreatePortal(type);
                        if (RedPortal != null) {
                            // linka os dois
                            BluePortal.linkedPortal = RedPortal;
                            RedPortal.linkedPortal = BluePortal;
                        }
                    }

                    // reposiciona o portal
                    BluePortal.transform.SetPositionAndRotation(
                        position: hit.collider.transform.position + hit.normal * 0.01f, 
                        rotation: Quaternion.LookRotation(hit.normal)
                    );
                    BluePortal.LinkedCollider = hit.collider;
                } else {
                    // cria o portal
                    if (RedPortal == null) {
                        RedPortal = CreatePortal(type);
                        if (BluePortal != null) {
                            // linka os dois
                            BluePortal.linkedPortal = RedPortal;
                            RedPortal.linkedPortal = BluePortal;
                        }
                    } 

                    // reposiciona o portal
                    RedPortal.transform.SetPositionAndRotation(
                        position: hit.collider.transform.position + hit.normal * 0.01f, 
                        rotation: Quaternion.LookRotation(-hit.normal)
                    );
                    RedPortal.LinkedCollider = hit.collider;
                }
            });
        }

        private Portal CreatePortal(PortalType type) {
            GameObject portal = Instantiate(portalPrefab, Vector3.zero, Quaternion.identity);
            portal.name = "Portal " + type;
            // definir cor certa do portal aqui
            return portal.GetComponent<Portal>();
        }

        public void ClearPortals() {
            Debug.Log("Limpando portais do usuario");
            if (BluePortal != null) {
                Destroy(BluePortal.gameObject);
                BluePortal = null;
            }
            if (RedPortal != null) {
                Destroy(RedPortal.gameObject);
                RedPortal = null;
            }

            // sinaliza pra arma fazer animacao de tururu
            gun.Fizzle();
        }

        /// <summary>
        /// Define se tava o controle do jogador ou nao.
        /// </summary>
        /// <param name="lockState"></param>
        public void SetLock(bool lockState) {
            isLocked = lockState;

            if (fpsController != null) {
                fpsController.BlockMovement = lockState;
            }
        }

        public void SetMute(bool muteState) {
            isMuted = muteState;
            audioListener.enabled = !muteState;
        }

        /// <summary>
        /// Aplica uma quantidade de dano ao jogador
        /// </summary>
        /// <param name="damageAmount"></param>
        public void InflictDamage(float damageAmount) {
            lastCombatTime = Time.time;
            currentHealth -= damageAmount;
            if(currentHealth <= 0) {
                currentHealth = 0;
                // morreu!
                Debug.Log("Morreu!");
                // TODO: eh o fim!!! MOVER ISSO NAO DEIXAR ISSO NO FINAL!!!
                Destroy(fpsController.gameObject);
            }
        }

        private void UpdateHealth() {
            if (Time.time - lastCombatTime > combatTimer && currentHealth < maxHealth) {
                currentHealth += regenRate * Time.deltaTime;
                if (currentHealth > maxHealth) {
                    currentHealth = maxHealth;
                }
            }
        }

        private void GrabUpdate() {
            if (grabbed != null && grabbed.TryGetComponent(out Rigidbody rb)) {
                var force = (grabPoint.position - grabbed.transform.position);
                Debug.DrawRay(grabbed.transform.position, force, Color.red);
                rb.AddForce(10 * force, ForceMode.Force);
            }
        }

        private void GrabObject() {
            if(grabbed != null) {
                // solta objeto
                gun.ReleaseObject();
                if(grabbed.TryGetComponent(out Rigidbody rb1)) {
                    // habilita fisica dnv
                    rb1.useGravity = true;
                    rb1.angularDamping = lastAngularDamping;
                    rb1.linearDamping = lastLinearDamping;
                }
                if(grabbedLastParent != null) { // pode estar na root
                    grabbed.transform.parent = grabbedLastParent.transform;
                } else {
                    grabbed.transform.parent = null;
                }
                grabbed = null;
                grabbedLastParent = null;
                return;
            }
            bool isHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, grabDistance);
            if (!isHit) {
                return;
            }

            // verifica se o objeto pode ser pego
            if (((1 << hit.collider.gameObject.layer) & grabbableLayer) == 0) {
                return;
            }

            gun.GrabObject();
            grabbed = hit.collider.gameObject;
            if(hit.collider.gameObject.transform.parent != null) {
                grabbedLastParent = hit.collider.gameObject.transform.parent.gameObject;
            }
            grabbed.transform.position = grabPoint.position;
            grabbed.transform.parent = grabPoint;

            //disable physics
            if (grabbed.TryGetComponent(out Rigidbody rb2)) {
                rb2.useGravity = false;
                lastAngularDamping = rb2.angularDamping;
                lastLinearDamping = rb2.linearDamping;
                //rb2.angularDamping = 1000;
                //rb2.linearDamping = 1000;
            }
        }

        private void OnDisable() {
            RenderPipelineManager.beginCameraRendering -= RenderPortals;
            if(BluePortal != null) {
                Destroy(BluePortal.gameObject);
            }
            if(RedPortal != null) {
                Destroy(RedPortal.gameObject);
            }
        }
    }
}