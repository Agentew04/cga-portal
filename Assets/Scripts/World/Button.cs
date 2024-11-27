using UnityEngine;
using System.Collections.Generic;

namespace PortalGame.World
{
    /// <summary>
    /// Classe que controla um botao no jogo
    /// </summary>
    public class Button : MonoBehaviour
    {
        [SerializeField]
        private Door linkedDoor;

        [SerializeField]
        private bool isActivated = false;

        [SerializeField, ColorUsage(true, true)]
        private Color blueEmission;

        [SerializeField, ColorUsage(true, true)]
        private Color orangeEmission;

        [SerializeField]
        private Animator anim;

        [SerializeField]
        private List<Renderer> mrs;

        public void Down() {
            if (isActivated) {
                return; 
            }
            Debug.Log("Down");
            isActivated = true;
            anim.ResetTrigger("Up");
            anim.SetTrigger("Down");
            
        }

        private void Update() {
            foreach (var mr in mrs) {
                mr.materials[1].SetColor("_EmissionColor", isActivated ? orangeEmission : blueEmission);
            }
        }

        public void Up() {
            if (!isActivated) {
                return;
            }
            Debug.Log("Up");
            isActivated = false;
            anim.ResetTrigger("Down");
            anim.SetTrigger("Up");
        }

        public void Trigger(Collider other, bool isEntering) {
            if (isEntering) {
                Down();
            } else {
                Up();
            }
        }
    }
}
