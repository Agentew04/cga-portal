using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.Player {

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
        private MeshRenderer coreGlow;

        [SerializeField]
        [Range(1, 10)]
        private float glowIntensity = 1.0f;

        [SerializeField]
        private ParticleSystem outerParticles;

        [SerializeField]
        private ParticleSystem projectileParticle;

        [SerializeField]
        private Transform desiredProjectilePosition;

        [SerializeField]
        private AudioSource audioSource;

        private ParticleSystemRenderer outerParticleRenderer;
        private ParticleSystemRenderer projectileParticleRenderer;
        private AudioManager audioMngr;
        private Animator animator;

        private void Start() {
            outerParticleRenderer = outerParticles.gameObject.GetComponent<ParticleSystemRenderer>();
            projectileParticleRenderer = projectileParticle.gameObject.GetComponent<ParticleSystemRenderer>();
            audioMngr = FindObjectOfType<AudioManager>();
            animator = GetComponent<Animator>();
        }

        private void Update() {
            projectileParticle.transform.position = desiredProjectilePosition.position;
            projectileParticle.transform.rotation = desiredProjectilePosition.rotation;
            innerLight.gameObject.SetActive(LastPortal != PortalType.None);
            outerParticles.gameObject.SetActive(LastPortal != PortalType.None);
            coreGlow.gameObject.SetActive(LastPortal != PortalType.None);

            Color color = LastPortal == PortalType.Blue ? blueColor : orangeColor;
            Color colorEmission = color * (Mathf.Pow(2, glowIntensity));
            innerLight.color = color;
            outerParticleRenderer.material.color = color;
            projectileParticleRenderer.material.color = color;
            coreGlow.material.SetColor("_EmissionColor", colorEmission);
        }

        /// <summary>
        /// Muda a cor da portal gun e toca audio
        /// </summary>
        /// <param name="portal"></param>
        public void Shoot(PortalType portal) {

            // atualiza portal
            LastPortal = portal;

            // toca som
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            if (portal == PortalType.Blue) {
                audioSource.clip = audioMngr.GetAudio(AudioType.GunShootBlue);
                audioSource.Play();
            } else if(portal == PortalType.Orange) {
                audioSource.clip = audioMngr.GetAudio(AudioType.GunShootOrange);
                audioSource.Play();
            }

            // lanca projetil
            projectileParticle.gameObject.SetActive(true); // ele se auto desliga
            projectileParticle.lights.light.color = portal == PortalType.Blue ? blueColor : orangeColor;
            if (projectileParticle.isPlaying) {
                projectileParticle.Stop();
            }
            projectileParticle.Play();

            // animacao do recoil
            animator.SetTrigger("Shoot");
        }
    }
}