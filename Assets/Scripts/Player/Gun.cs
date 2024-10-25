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

        public enum GunSound {
            ShootBluePortal,
            ShootOrangePortal,
        }

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

        [SerializeField]
        private AudioSource audioSource;

        private ParticleSystemRenderer particleRenderer;
        private AudioManager audioMngr;

        private void Start() {
            particleRenderer = outerParticles.gameObject.GetComponent<ParticleSystemRenderer>();
            audioMngr = FindObjectOfType<AudioManager>();
        }

        private void Update() {
            innerLight.gameObject.SetActive(LastPortal != PortalType.None);
            outerParticles.gameObject.SetActive(LastPortal != PortalType.None);

            if (LastPortal == PortalType.Blue) {
                innerLight.color = blueColor;
                particleRenderer.material.color = blueColor;
            }else if(LastPortal == PortalType.Orange){
                innerLight.color = orangeColor;
                particleRenderer.material.color = orangeColor;
            }
        }

        /// <summary>
        /// Muda a cor da portal gun e toca audio
        /// </summary>
        /// <param name="portal"></param>
        public void Shoot(PortalType portal) {

            LastPortal = portal;
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            if(portal == PortalType.Blue) {
                audioSource.clip = audioMngr.GetAudio(AudioType.GunShootBlue);
                audioSource.Play();
            } else if(portal == PortalType.Orange) {
                audioSource.clip = audioMngr.GetAudio(AudioType.GunShootOrange);
                audioSource.Play();
            }
        }
    }
}