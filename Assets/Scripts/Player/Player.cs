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

        [field: SerializeField]
        public Portal RedPortal { get; set; } = null;

        [field: SerializeField]
        public Portal BluePortal { get; set; } = null;

        private PlayerMovement playerMovement;
        private MouseLook mouseLook;
        private FirstPersonController fpsController;

        private void Start() {
            RenderPipelineManager.beginCameraRendering += RenderPortals;
            playerMovement = FindAnyObjectByType<PlayerMovement>();
            mouseLook = FindAnyObjectByType<MouseLook>();
            fpsController = FindAnyObjectByType<FirstPersonController>();
        }

        private void Update() {

            if (Input.GetKeyDown(KeyCode.F1)) {
                if(playerMovement != null && mouseLook != null) {
                    playerMovement.BlockMovement = !playerMovement.BlockMovement;
                    mouseLook.BlockMovement = !mouseLook.BlockMovement;
                }else if(fpsController != null) {
                    fpsController.cameraCanMove = !fpsController.cameraCanMove;
                    fpsController.playerCanMove = !fpsController.playerCanMove;
                }
                pauseMenu.TogglePause();
            }

            if(Input.GetMouseButtonDown(0)){
                Click(PortalType.Blue);
            } else if (Input.GetMouseButtonDown(1)) {
                Click(PortalType.Orange);
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
            if((mouseLook != null && playerMovement != null &&
                (mouseLook.BlockMovement || playerMovement.BlockMovement))
                || (fpsController != null && (!fpsController.cameraCanMove || !fpsController.playerCanMove))) {
                return;
            }

            Ray ray = new(transform.position, transform.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, 1000.0f);

            if(!isHit) {
                return;
            }

            bool validSurface = hit.collider.gameObject.layer == LayerMask.NameToLayer("Portalable");
            
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

            // avisa a torreta
            Turret[] turrets = FindObjectsByType<Turret>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach(Turret turret in turrets) {
                turret.GiveHint(hit.point);
            }
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