using UnityEngine;

namespace PortalGame.Player {
    public class ParticleListener : MonoBehaviour {

        [Header("Referencias")]
        [SerializeField]
        private Gun gun;

        private void OnParticleCollision() {
            if(gun == null) {
                Debug.LogError("Gun nao foi referenciada");
                return;
            }
            gun.OnParticleCollided();
        }
    }
}

