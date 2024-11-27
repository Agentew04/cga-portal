using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PortalGame.Menu
{
    /// <summary>
    /// Classe que representa um botao do menu lateral principal do jogo.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        private Button but;

        [SerializeField]
        private View destinationView;

        [SerializeField]
        private UnityEvent action;


        private void Awake() {
            but = GetComponent<Button>();
            but.onClick.AddListener(OnClick);
        }

        private void OnClick() {
            if(destinationView != null) {
                destinationView.Menu.CurrentViewIndex = destinationView.Index;
            }
            action?.Invoke();
        }

        private void OnDestroy() {
            but.onClick.RemoveListener(OnClick);
        }
    }
}
