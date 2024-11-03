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
        private float audioDelay = 0.1f;

        [Header("Referencias")]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource audioSource;

        private Player.Player player;

        public bool IsOpen { get; private set; } = false;

        private void Start() {
            player = FindObjectOfType<Player.Player>();
        }

        private void Update() {
            if(player == null) {
                player = FindObjectOfType<Player.Player>();
            }
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (useProximity && distance <= openingDistance && !IsOpen) {
                Open();
            }else if(useProximity && distance > openingDistance && IsOpen) {
                Close();
            }
        }

        public void Open() {
            animator.SetTrigger("Open");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.DoorOpen);
            audioSource.PlayDelayed(audioDelay);
            IsOpen = true;
        }

        public void Close() {
            animator.SetTrigger("Close");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.DoorClose);
            audioSource.Play();
            IsOpen = false;
        }
    }
}
