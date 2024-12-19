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
        public Door EndDoor { get; private set; }


        [field: SerializeField]
        public Door StartDoor { get; set; }

        public LevelManager Manager { get; set; }

        private bool alreadyRequestedLoad = false;

        private void Start() {
            // a porta do inicio comeca desligada pois
            // usamos a do cenario anterior
            if(StartDoor != null) {
                StartDoor.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Chamado quando o jogador para pelo trigger
        /// </summary>
        /// <param name="other"></param>
        /// <param name="isEntering"></param>
        public void LoadNextLevel(Collider other, bool isEntering) {
            if (!isEntering) {
                return;
            }

            // check layer
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) {
                return;
            }

            if (alreadyRequestedLoad) {
                return;
            }

            alreadyRequestedLoad = true;

            // verifica pois sala inicial nao tem porta de entrada
            if(StartDoor != null) {
                StartDoor.gameObject.SetActive(true);
                StartDoor.Close();
                StartDoor.IsLocked = true;
            }
            Manager.LoadNextLevel(() => {
                // destrava a porta do final do corredior
                EndDoor.IsLocked = false;
            });
        }
    }
}
