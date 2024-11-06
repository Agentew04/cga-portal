using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.World {
    [RequireComponent(typeof(Light))]
    public class BlinkingLight : MonoBehaviour {

        private Light light;

        [Header("Configuracoes")]
        [SerializeField, Range(0,1)]
        private float chance = 0.0f;

        void Start() {
            light = GetComponent<Light>();
        }

        private void Update() {
            if (Random.value <= chance) {
                light.enabled = !light.enabled;
            }
        }
    }
}
