using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PortalGame.World;
using System.Transactions;
using UnityEditor.Experimental.GraphView;
using UnityEngine.InputSystem.HID;

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
        private Material damageMaterial;

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
            EnsureUpright();

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

        /// <summary>
        /// Renderiza os portais e suas recursoes etc.
        /// </summary>
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

        /// <summary>
        /// Callback quando o jogador clica para atirar num portal
        /// </summary>
        /// <param name="type">O tipo do portal atirado(BEM ou BDM)</param>
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

            Portal portal = type == PortalType.Blue ? BluePortal : RedPortal;

            validSurface = CheckPortalPlacement(portal, type, ref hit, validSurface, out var portalWallColliders);

            Debug.DrawLine(ray.origin, hit.point, Color.red, 5);
            gun.ShootProjectile(type, validSurface, () => {
                if (!validSurface) {
                    return;
                }
                // cria portal se nao existia
                if(portal == null) {
                    portal = CreatePortal(type);
                    if(type == PortalType.Blue) {
                        BluePortal = portal;
                    } else {
                        RedPortal = portal;
                    }
                    if (BluePortal != null && RedPortal != null) {
                        BluePortal.linkedPortal = RedPortal;
                        RedPortal.linkedPortal = BluePortal;
                    }
                }

                // reposiciona o portal
                portal.transform.SetPositionAndRotation(
                    position: hit.point + hit.normal * 0.05f,
                    rotation: Quaternion.LookRotation((type == PortalType.Orange ? -1 : 1) * hit.normal)
                );
                portal.LinkedColliders.Clear();
                portal.LinkedColliders.AddRange(portalWallColliders);
            });
        }

        /// <summary>
        /// Verifica se o portal realmente cabe no lugar que o jogador atirou
        /// e se todas as superficies sao validas
        /// </summary>
        /// <param name="portal">O portal atual. Pode ser null</param>
        /// <param name="type">O tipo de portal que o jogador atirou</param>
        /// <param name="hit">O resultado do raycast</param>
        /// <param name="validSurface">Se a superficie batida eh valida ou nao</param>
        /// <param name="portalWallColliders">A lista dos colisores atras do portal</param>
        /// <returns>Se o portal pode ser colocado ali ou nao</returns>
        private bool CheckPortalPlacement(Portal portal, PortalType type, ref RaycastHit hit, 
                bool validSurface, out List<Collider> portalWallColliders) {
            if (!validSurface) {
                portalWallColliders = new();
                return false;
            }
            // save old values
            Vector3 oldPosition = portal != null ? portal.transform.position : Vector3.zero;
            Quaternion oldRotation = portal != null ? portal.transform.rotation : Quaternion.identity;
            // move and rotate to new position
            bool wasnull = portal == null;
            if (portal == null) {
                wasnull = true;
                portal = CreatePortal(type);
                portal.PortalType = type;
            }
            var rot = (type == PortalType.Orange ? -1 : 1) * hit.normal;
            portal.transform.SetPositionAndRotation(hit.point + hit.normal * 0.05f, Quaternion.LookRotation(rot, Vector3.up));
            Physics.SyncTransforms();
            if (!PortalFits(portal, hit.normal, out portalWallColliders)) {
                validSurface = false;
            }
            if (wasnull) {
                // destroi portal de testes
                Destroy(portal.gameObject);
            } else {
                // volta para a posicao original
                portal.transform.SetPositionAndRotation(oldPosition, oldRotation);
            }
            Physics.SyncTransforms();
            return validSurface;
        }

        /// <summary>
        /// Cria um portal novo a partir do prefab e seta o tipo dele.
        /// </summary>
        /// <param name="type">O tipo do portal a ser criado</param>
        /// <returns>O portal criado</returns>
        private Portal CreatePortal(PortalType type) {
            GameObject portal = Instantiate(portalPrefab, Vector3.zero, Quaternion.identity);
            portal.name = "Portal " + type;
            Portal p = portal.GetComponent<Portal>();
            p.PortalType = type;
            return p;
        }

        /// <summary>
        /// Limpa quaisquer portais jogados pelo player
        /// </summary>
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
        /// Verifica se um portal cabe num lugar
        /// </summary>
        /// <param name="p">O portal a ser testado</param>
        /// <param name="normal">A normal da superficie</param>
        /// <param name="colliders">Saida da lista dos colisores atingidos</param>
        /// <returns></returns>
        private static bool PortalFits(Portal p, Vector3 normal, out List<Collider> colliders) {
            // draw the p position
            Debug.DrawRay(p.PortalBorderCollider.transform.position, p.PortalBorderCollider.transform.right, Color.red, 1);
            Debug.DrawRay(p.PortalBorderCollider.transform.position, p.PortalBorderCollider.transform.up, Color.green, 1);
            Debug.DrawRay(p.PortalBorderCollider.transform.position, p.PortalBorderCollider.transform.forward, Color.blue, 1);
            var corners = p.PortalBorderCollider.bounds.GetCorners();
            bool anyFalse = false;
            colliders = new();
            foreach (var corner in corners) {
                // raycast na direcao do portal
                Ray ray = new Ray(corner, -normal);
                LayerMask layer = LayerMask.GetMask("Portalable");
                bool isHit = Physics.Raycast(ray, out RaycastHit hit, 1.0f, layer);
                Debug.DrawRay(ray.origin, ray.direction, isHit ? Color.green : Color.red, 5);
                if (!isHit) {
                    anyFalse = true;
                } else {
                    // hitou
                    if (!colliders.Contains(hit.collider)) {
                        colliders.Add(hit.collider);
                    }
                }
            }
            return !anyFalse;
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

        /// <summary>
        /// Determina se o jogador ouve audio ou nao.
        /// Eh para a cena de loading, o player n ouvir a porta fechando
        /// </summary>
        /// <param name="muteState"></param>
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
                Debug.Log("Morreu!");
                pauseMenu.ShowDeathScreen();
                Destroy(fpsController.gameObject); // bye bye player
                // LevelManager se encarrega de nos spawnar dnv
            }
        }

        /// <summary>
        /// Cuida se o jogador já está apto a regenerar vida
        /// </summary>
        private void UpdateHealth() {
            // isso ta aqui soh pra teste, nao precisaria na teoria
            if(currentHealth <= 0) {
                Debug.Log("Morreu!");
                pauseMenu.ShowDeathScreen();
                Destroy(fpsController.gameObject); // bye bye player
            }
            
            // atualiza material de dano!
            // 0: 100 health
            // 1: 0 health
            float fade = 1 - currentHealth / maxHealth;
            damageMaterial.SetFloat("_FadeValue", fade);

            if (Time.time - lastCombatTime > combatTimer && currentHealth < maxHealth) {
                currentHealth += regenRate * Time.deltaTime;
                if (currentHealth > maxHealth) {
                    currentHealth = maxHealth;
                }
            }

        }

        /// <summary>
        /// Garante que o jogador esteja sempre em pe, depois
        /// de passar por um portal
        /// </summary>
        private void EnsureUpright() {
            // rotacao do fpsController sempre é projetada no chao
            // get upwards quaternion
            Vector3 forwardProjected = Vector3.ProjectOnPlane(fpsController.transform.forward, Vector3.up).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, Vector3.up);

            // se o angulo entre o up e o player for != 90, tem q ajustar
            float angle = Quaternion.Angle(fpsController.transform.rotation, targetRotation);
            float tolerance = 0.1f;
            if (angle > tolerance) {
                // Rotaciona o player suavemente para o alvo
                fpsController.transform.rotation = Quaternion.Slerp(fpsController.transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

        }

        /// <summary>
        /// Move a caixa que o jogador esta segurando
        /// para a posicao desejada. 
        /// </summary>
        /// <remarks>Ela vai ficar flutuando tipo fantasma. 
        /// Talvez usar as joints igual na RMD?
        /// </remarks>
        private void GrabUpdate() {
            if (grabbed != null && grabbed.TryGetComponent(out Rigidbody rb)) {
                var force = (grabPoint.position - grabbed.transform.position);
                Debug.DrawRay(grabbed.transform.position, force, Color.red);
                rb.AddForce(10 * force, ForceMode.Force);
            }
        }

        /// <summary>
        /// Pega um objeto
        /// </summary>
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