using System;
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

        [SerializeField]
        private AnimationCurve innerGlowCurve;

        [SerializeField]
        private AnimationCurve indicatorGlowCurve;


        [Header("Referencias")]
        [SerializeField]
        private Light innerLight;

        [SerializeField]
        private MeshRenderer coreGlow;

        [SerializeField]
        private MeshRenderer indicatorGlow;

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
        private Animator animator;
        private Action projectileCallback;

        private void Start() {
            outerParticleRenderer = outerParticles.gameObject.GetComponent<ParticleSystemRenderer>();
            projectileParticleRenderer = projectileParticle.gameObject.GetComponent<ParticleSystemRenderer>();
            animator = GetComponent<Animator>();
        }

        private void Update() {
            projectileParticle.transform.position = desiredProjectilePosition.position;
            projectileParticle.transform.rotation = desiredProjectilePosition.rotation;
            UpdateLightEmission(LastPortal == PortalType.Blue ? blueColor : orangeColor);
        }

        private void UpdateLightEmission(Color color) {
            float innerGlowIntensity = innerGlowCurve.Evaluate(Time.time);
            float outerGlowIntensity = indicatorGlowCurve.Evaluate(Time.time);
            Color innerEmission = color * Mathf.Pow(2, innerGlowIntensity);
            Color outerEmission = color * Mathf.Pow(2, outerGlowIntensity);
            coreGlow.material.SetColor("_EmissionColor", innerEmission);
            indicatorGlow.material.SetColor("_EmissionColor", outerEmission);
        }

        /// <summary>
        /// Muda a cor da portal gun, toca audio e joga
        /// o projetil do portal. Invoca callback quando
        /// a particula colide.
        /// </summary>
        /// <param name="portal">O tipo de portal jogado</param>
        /// <param name="callback">Callback chamado quando a particula colide</param>
        /// <param name="validSurface">Se a superficie atingida eh valida para o portal</param>
        public void ShootProjectile(PortalType portal, bool validSurface, Action callback) {
            projectileCallback = callback;

            // atualiza portal
            LastPortal = portal;

            if (audioSource.isPlaying) {
                audioSource.Stop();
            }

            // define o som que vai tocar
            Debug.Log("Is valid: " + validSurface);
            if(!validSurface) {
                audioSource.clip = AudioManager.Instance.GetAudio(AudioType.GunShootInvalidSurface);
                Debug.Log("Invalid surface Audio");
            }else if (portal == PortalType.Blue) {
                audioSource.clip = AudioManager.Instance.GetAudio(AudioType.GunShootBlue);
                Debug.Log("Blue audio");
            } else if(portal == PortalType.Orange) {
                audioSource.clip = AudioManager.Instance.GetAudio(AudioType.GunShootOrange);
                Debug.Log("Orange audio");
            } else {
                Debug.Log("no audio");
                audioSource.clip = null;
            }
            audioSource.Play();

            // lanca projetil
            projectileParticle.gameObject.SetActive(true); // ele se auto desliga             
            projectileParticle.lights.light.color = portal == PortalType.Blue ? blueColor : orangeColor;
            if (projectileParticle.isPlaying) {
                projectileParticle.Stop();
            }
            projectileParticle.Play();

            // animacao do recoil
            animator.SetTrigger("Shoot");

            // muda a cor da arma
            UpdateGunColor();
        }
    
        /// <summary>
        /// Chamado pelo script <see cref="ParticleListener"/>.
        /// </summary>
        public void OnParticleCollided() {
            projectileCallback?.Invoke();
        }    

        private void UpdateGunColor() {
            innerLight.gameObject.SetActive(LastPortal != PortalType.None);
            coreGlow.gameObject.SetActive(LastPortal != PortalType.None);
            indicatorGlow.gameObject.SetActive(LastPortal != PortalType.None);

            Color color = LastPortal == PortalType.Blue ? blueColor : orangeColor;
            innerLight.color = color;
            projectileParticleRenderer.material.color = color;
            UpdateLightEmission(color);
        }
    }
}