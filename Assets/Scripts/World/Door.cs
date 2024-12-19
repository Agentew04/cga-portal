using PortalGame.Player;
using System;
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

        private FpsController fpsController;

        public bool IsOpen { get; private set; } = false;

        [field: SerializeField]
        public bool IsLocked { get; set; } = false;

        [field: SerializeField]
        public Transform BackSide { get; set; }

        [SerializeField]
        private MeshRenderer frontCover;

        [SerializeField]
        private MeshRenderer backCover;

        public Material FrontMaterial {
            get => frontCover.material;
            set => frontCover.material = value;
        }

        public Material BackMaterial {
            get => backCover.material;
            set => backCover.material = value;
        }

        private void Start() {
            fpsController = FindAnyObjectByType<FpsController>();
        }

        private void Update() {
            if(fpsController == null) {
                return; // previne erro quando o jogador morre
            }
            Transform player = fpsController.transform;
            float distance = Vector3.Distance(player.position, transform.position);
            AutoOpenSide side = GetSide(player);
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
            if(!IsOpen) {
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
