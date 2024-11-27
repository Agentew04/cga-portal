using UnityEngine;

namespace PortalGame.Menu
{
    /// <summary>
    /// Classe que representa uma view do <see cref="ModularMenu"/>.
    /// </summary>
    public class View : MonoBehaviour
    {
        [field: Header("Configuracoes")]
        [field: SerializeField]
        public string ViewName { get; set; }

        [field: SerializeField]
        public ModularMenu Menu { get; set; }
        

        public int Index { get; set; } = -2;

        
    }
}
