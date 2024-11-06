using System.Collections;
using System.Collections.Generic;
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

        // a porta de inicio do nivel pertence ao corredor de loading
        [field: SerializeField]
        public Transform StartDoorPosition { get; set; }

        // a porta do fim do nivel pertence ao nivel
        [field: SerializeField]
        public Transform EndDoorPosition { get; set; }
    }
}
