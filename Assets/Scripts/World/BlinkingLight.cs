using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.World {
    [RequireComponent(typeof(Light))]
    public class BlinkingLight : MonoBehaviour {

        private Light light;

        [Header("Configuracoes")]
        [SerializeField, Tooltip("De quantos em quantos segundos tentar atualizar status")]
        private float updateRate = 0.1f;

        [SerializeField, Range(0,1), Tooltip("Chance da luz mudar de estado num update")]
        private float chance = 0.0f;

        private float lastUpdate = 0;

        void Start() {
            light = GetComponent<Light>();
        }

        private void Update() {
            if (Time.time - lastUpdate >= updateRate) {
                lastUpdate = Time.time;
                if (Random.value <= chance) {
                    light.enabled = !light.enabled;
                }
            }
        }
    }
}
