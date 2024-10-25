using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portal.Player {
    public class Player : MonoBehaviour {

        [Header("Referencias")]
        [SerializeField]
        private Gun gun;

        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if(Input.GetMouseButtonDown(0)){
                gun.Shoot(PortalType.Blue);
            }else if (Input.GetMouseButtonDown(1)) {
                gun.Shoot(PortalType.Orange);
            }
        }
    }
}