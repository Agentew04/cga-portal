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
                Debug.Log("Abrindo porta");
                Open();
            }else if(useProximity && distance > openingDistance && IsOpen) {
                Debug.Log("Fechando porta");
                Close();
            }

            //// tenta corrigir desync
            //if(IsOpen 
            //    && !animator.GetCurrentAnimatorStateInfo(0).IsName("Opening")
            //    && !animator.GetCurrentAnimatorStateInfo(0).IsName("Open")) {
            //    Debug.Log("Corrigindo pra open");
            //    animator.SetTrigger("Open");
            //} else if (!IsOpen 
            //    && !animator.GetCurrentAnimatorStateInfo(0).IsName("Closing")
            //    && !animator.GetCurrentAnimatorStateInfo(0).IsName("Closed")) {
            //    Debug.Log("Corrigindo pra closed");
            //    animator.SetTrigger("Close");
            //}
        }

        public void Open() {
            animator.SetTrigger("Open");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.DoorOpen);
            audioSource.Play();
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
