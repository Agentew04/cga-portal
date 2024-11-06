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
    }
}
