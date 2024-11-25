using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.World {

    /// <summary>
    /// Script usado para o corredor de loading.
    /// Basicamente so eh usado para indicar a 
    /// posicao final da porta pra conectar
    /// no proximo nivel.
    /// </summary>
    public class WaitRoom : MonoBehaviour {

        /// <summary>
        /// A posicao onde fica a porta do fim do corredor.
        /// </summary>
        [field: SerializeField]
        public Transform EndDoorPosition { get; private set; }

        [SerializeField]
        private Door endDoor;

        [field: SerializeField, Tooltip("Referencia a porta do nivel anterior")]
        public Door StartDoor { get; set; }

        public LevelManager Manager { get; set; }

        public void LoadNextLevel(Collider other, bool isEntering) {
            if (!isEntering) {
                return;
            }

            // check layer
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) {
                return;
            }

            StartDoor.Close();
            StartDoor.IsLocked = true;
            Manager.LoadNextLevel(() => {
                // destrava a porta do final do corredior
                endDoor.IsLocked = false;
            });
        }
    }
}
