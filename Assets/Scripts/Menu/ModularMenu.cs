using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PortalGame.Menu
{
    /// <summary>
    /// Script que controla a animacao e troca de <see cref="View"/> 
    /// do menu lateral. Nao possui logica de negocios.
    /// </summary>
    public class ModularMenu : MonoBehaviour
    {
        [Header("Configuracoes")]
        [SerializeField, Tooltip("Lista com as views do menu")]
        private List<View> views;

        [SerializeField, Tooltip("A primeira view a ser carregada")]
        private int defaultViewIndex;

        [SerializeField, Tooltip("Apenas para visualizacao")]
        private int currentViewIndex;

        [Header("Referencias")]
        [SerializeField, Tooltip("Referencia ao text titulo")]
        private TextMeshProUGUI titleLabel;

        [SerializeField, Tooltip("Referencia ao controlador de tiles")]
        private TiledMenu tiledMenu;

        /// <summary>
        /// Determina qual a <see cref="View"/> que vai ser
        /// mostrada no menu. Automaticamente atualiza a UI
        /// quando o valor eh alterado.
        /// </summary>
        public int CurrentViewIndex {
            get => currentViewIndex;
            set {
                if(currentViewIndex != value) {
                    OnViewChanged(currentViewIndex, value);
                    currentViewIndex = value;
                }
            }
        }

        private void Start() {
            currentViewIndex = defaultViewIndex;
            foreach (View v in views) {
                v.gameObject.SetActive(false);
            }
            views[defaultViewIndex].gameObject.SetActive(true);

            int i = 0;
            foreach (View v in views) {
                v.Index = i++;
            }
        }

        private void OnViewChanged(int oldValue, int newValue) {
            // desativa antigo
            if(oldValue != -1 && views.Count > oldValue) {
                View vOld = views[oldValue];
                vOld.gameObject.SetActive(false);
            }
            if(newValue != -1 && views.Count > newValue) {
                View vNew = views[newValue];
                vNew.gameObject.SetActive(true);
                titleLabel.text = vNew.ViewName;
            }

            if ((oldValue != -1 && views.Count > oldValue) 
                || (newValue != -1 && views.Count > newValue)
                /*&& Time.frameCount > 1*/) {
                if (!tiledMenu.Turn(GetComponent<RectTransform>())){
                    Debug.Log("N consegui virar");
                }
            }
        }
    }
}
