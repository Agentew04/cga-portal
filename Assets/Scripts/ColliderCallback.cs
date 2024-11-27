using UnityEngine;
using UnityEngine.Events;

namespace PortalGame
{
    [RequireComponent(typeof(Collider))]
    public class ColliderCallback : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<Collider,bool> callback;

        private void OnCollisionEnter(Collision collision) {
            callback.Invoke(collision.collider, true);
        }

        private void OnCollisionExit(Collision collision) {
            callback.Invoke(collision.collider, false);
        }
    }
}
