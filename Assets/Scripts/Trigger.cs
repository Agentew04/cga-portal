using UnityEngine;
using UnityEngine.Events;

namespace PortalGame {

    /// <summary>
    /// Classe que representa um trigger generico.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Trigger : MonoBehaviour {

        private new Collider collider;

        [SerializeField]
        private UnityEvent<Collider, bool> onTrigger;

        private void Start() {
            if (!TryGetComponent(out collider)) {
                Debug.LogError("Collider nao encontrado no trigger.");
                return;
            }

            if (!collider.isTrigger) {
                Debug.LogWarning("Collider do trigger nao esta como trigger. Corrigindo...");
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other) {
            onTrigger?.Invoke(other, true);
        }

        private void OnTriggerExit(Collider other) {
            onTrigger?.Invoke(other, false);
        }
    }
}
