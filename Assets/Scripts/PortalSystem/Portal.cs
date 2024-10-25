using PortalGame.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.PortalSystem {

    /// <summary>
    /// Representa um portal no jogo. Eh pra ter
    /// apenas 2 portal em qualquer dado momento.
    /// Responsabilidade de <see cref="Player.Player"/>
    /// gerenciar eles.
    /// </summary>
    public class Portal : MonoBehaviour {

        /// <summary>
        /// Necessario pois esse portal controla a camera
        /// do outro e vice versa
        /// </summary>
        public Portal LinkedPortal { get; set; }

        /// <summary>
        /// A camera que renderiza a tela desse portal
        /// </summary>
        [field: SerializeField]
        public Camera Camera { get; set; }

        /// <summary>
        /// Mesh que vai renderizar a visao
        /// do outro portal. Basicamente eh a area
        /// em si do portal que vemos
        /// </summary>
        [SerializeField]
        private MeshRenderer screen;

        /// <summary>
        /// Textura que a <see cref="Camera"/> renderiza.
        /// </summary>
        private RenderTexture renderTexture;

        private void Start() {
        }

        private void OnDestroy() {
            
        }
    }
}
