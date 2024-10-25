using Portal.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portal {

    public enum AudioType {
        GunShootBlue,
        GunShootOrange,
    }

    public class AudioManager : MonoBehaviour {

        [SerializeField]
        private UnityDictionary<AudioType, AudioClip> m_AudioSources = new();

        public AudioClip GetAudio(AudioType key) {
            return m_AudioSources[key];
        }
    }
}