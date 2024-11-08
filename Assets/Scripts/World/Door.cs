using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.World {
    /// <summary>
    /// Classe que gerencia uma porta no jogo
    /// </summary>
    public class Door : MonoBehaviour {

        [Header("Configuracoes")]
        [SerializeField]
        private bool useProximity = false;

        [SerializeField]
        private float openingDistance = 2.0f;

        [SerializeField]
        private AutoOpenSide autoOpenSide;

        [SerializeField]
        private float audioDelay = 0.1f;

        [Header("Referencias")]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource audioSource;

        private FirstPersonController player;

        public bool IsOpen { get; private set; } = false;

        [field: SerializeField]
        public bool IsLocked { get; set; } = false;

        [field: SerializeField]
        public Transform BackSide { get; set; }

        private void Start() {
            player = FindObjectOfType<FirstPersonController>();
        }

        private void Update() {
            if(player == null) {
                player = FindObjectOfType<FirstPersonController>();
            }
            if (player == null) {
                return;
            }
            float distance = Vector3.Distance(player.transform.position, transform.position);
            AutoOpenSide side = GetSide(player.transform);
            if (useProximity && distance <= openingDistance && !IsOpen && (side & autoOpenSide) != AutoOpenSide.None) {
                Open();
            }else if(useProximity && distance > openingDistance && IsOpen) {
                Close();
            }
        }

        public void Open() {
            if (IsLocked) {
                return;
            }
            animator.SetTrigger("Open");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.DoorOpen);
            audioSource.PlayDelayed(audioDelay);
            IsOpen = true;
        }

        public void Close() {
            if (IsLocked) {
                return;
            }
            animator.SetTrigger("Close");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.DoorClose);
            audioSource.Play();
            IsOpen = false;
        }

        private AutoOpenSide GetSide(Transform other) {
            Vector3 doorLocal = transform.localPosition;
            Vector3 otherLocal = transform.InverseTransformPoint(other.position);
            float dot = Vector3.Dot(doorLocal, otherLocal);
            return SignToSide((int)Mathf.Sign(dot));
        }

        private static AutoOpenSide SignToSide(int sign) {
            if (sign == 0) {
                return AutoOpenSide.None;
            }
            return sign > 0 ? AutoOpenSide.Front : AutoOpenSide.Back;
        }

        [Flags, Serializable]
        public enum AutoOpenSide {
            None = 0,
            Front = 1,
            Back = 2,
        }
    }
}
