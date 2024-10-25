using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portal.Player {

    public enum PortalType {
        None,
        Blue,
        Orange
    }

    public class Gun : MonoBehaviour {

        [field: Header("Configuracoes")]
        [field: SerializeField]
        public PortalType LastPortal { get; set; } = PortalType.Blue;

        [SerializeField]
        private Color blueColor = new(0.0f, 0.0f, 1.0f, 1.0f);

        [SerializeField]
        private Color orangeColor = new(1.0f, 0.5f, 0.0f, 1.0f);

        [Header("Referencias")]
        [SerializeField]
        private Light innerLight;

        [SerializeField]
        private ParticleSystem outerParticles;

        private ParticleSystemRenderer particleRenderer;

        private void Start() {
            particleRenderer = outerParticles.gameObject.GetComponent<ParticleSystemRenderer>();
        }

        private void Update() {
            innerLight.enabled = LastPortal != PortalType.None;
            outerParticles.gameObject.SetActive(LastPortal != PortalType.None);

            if (LastPortal == PortalType.Blue) {
                innerLight.color = blueColor;
                particleRenderer.material.color = blueColor;
            }else if(LastPortal == PortalType.Orange){
                innerLight.color = orangeColor;
                particleRenderer.material.color = orangeColor;
            }
        }
    }
}