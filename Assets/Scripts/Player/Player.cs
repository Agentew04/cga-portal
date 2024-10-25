using PortalGame.PortalSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// deixa usar '?' para objetos que podem ser nulos
#nullable enable

namespace PortalGame.Player {

    /// <summary>
    /// Classe 
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Player : MonoBehaviour {

        [Header("Referencias")]
        [SerializeField]
        private Gun gun;

        [field: SerializeField]
        public Portal? RedPortal { get; set; } = null;

        [field: SerializeField]
        public Portal? BluePortal { get; set; } = null;

        private void Start() {

        }

        private void Update() {

            if(Input.GetMouseButtonDown(0)){
                Click(PortalType.Blue);
            } else if (Input.GetMouseButtonDown(1)) {
                Click(PortalType.Orange);
            }
        }

        private void OnPreCull() {
            if(BluePortal == null || RedPortal == null) {
                return;
            }
            //BluePortal.PrePortalRender();
            //RedPortal.PrePortalRender();

            //BluePortal.Render();
            //RedPortal.Render();

            //BluePortal.PostPortalRender();
            //RedPortal.PostPortalRender();
        }

        private void Click(PortalType type) {
            Ray ray = new(transform.position, transform.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, 1000.0f);

            if(!isHit) {
                return;
            }

            Debug.Log("Hit: " + hit.collider.gameObject.name);
            Debug.DrawLine(ray.origin, hit.point, Color.red, 5);
            gun.Shoot(type); // toca som e muda cor da arma
            if(type == PortalType.Blue) {
                if(BluePortal == null) {
                    BluePortal = CreatePortal();
                } else {
                    // apenas reposiciona o portal
                    BluePortal.transform.position = hit.point;
                }
            } else {
                if (RedPortal == null) {
                    RedPortal = CreatePortal();
                } else {
                    // apenas reposiciona o portal
                    RedPortal.transform.position = hit.point;
                }
            }
        }

        private Portal CreatePortal() {
            return null;
        }
    }
}