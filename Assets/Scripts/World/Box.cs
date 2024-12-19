using UnityEngine;

namespace PortalGame
{
    /// <summary>
    /// Script que toca um som quando a caixa colide com algo.
    /// </summary>
    public class Box : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        private void OnCollisionEnter(Collision collision) {
            Debug.Log("colisao cubo entrada");
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            audioSource.clip = AudioManager.Instance.GetAudio(AudioType.BoxHit);
            audioSource.Play();
        }
    }
}
