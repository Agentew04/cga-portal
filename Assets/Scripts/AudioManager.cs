using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PortalGame {

    /// <summary>
    /// Enumerador que define os tipos de audio disponiveis
    /// no jogo. Usado na funcao <see cref="AudioManager.GetAudio(AudioType)"/>.
    /// </summary>
    public enum AudioType {
        None,
        GunShootBlue,
        GunShootOrange,
        GunShootInvalidSurface,
        PortalEnter,
        PortalBlueOpen,
        PortalOrangeOpen,
        DoorOpen,
        DoorClose,
        GunFizzle,
    }

    /// <summary>
    /// Classe que gerencia os audios do jogo.
    /// Acessar atraves de <see cref="Instance"/>.
    /// </summary>

    public class AudioManager : MonoBehaviour {

        private static AudioManager instance;
        public static AudioManager Instance {
            get => instance;
            set {
                if (instance == null) {
                    instance = value;
                } else {
                    if(value != null) {
                        Destroy(value.gameObject);
                    }
                }
            }
        }

        private void Awake() {
            Instance = this;
        }

        [SerializeField]
        private UnityDictionary<AudioType, AudioClip> m_AudioSources = new();

        /// <summary>
        /// Retorna aleatoriamente um dos audios relacionados
        /// ao tipo requisitado.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AudioClip GetAudio(AudioType key) {
            if(key == AudioType.None) {
                return null;
            }

            var availableAudios = m_AudioSources.Where(x => x.Key == key);
            if (!availableAudios.Any()) {
                throw new KeyNotFoundException();
            }

            return availableAudios.ElementAt(Random.Range(0, availableAudios.Count())).Value;
        }

        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }

        private void OnEnable() {
            if (Instance == null) {
                Instance = this; // auto destroi se ja houver
            }
        }

        private void OnDisable() {
            if (Instance == this) {
                Instance = null;
            }
        }
    }
}