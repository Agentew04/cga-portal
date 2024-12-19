using UnityEngine;

namespace PortalGame.World {

    /// <summary>
    /// Classe que simboliza um unico nivel do jogo
    /// </summary>
    public class Level : MonoBehaviour {

        [field: SerializeField]
        public int LevelIndex { get; set; }

        /*
         * As duas portas sao usadas para conectar
         * niveis e os corredores corretamente
         */
        
        [field: SerializeField]
        public Door StartDoor { get; set; }

        [field: SerializeField]
        public Door EndDoor { get; set; }
    }
}
