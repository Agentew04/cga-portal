using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PortalGame.Menu {
    public class A : MonoBehaviour {

        [SerializeField]
        private Image back1;

        [SerializeField]
        private Sprite spr1;
        [SerializeField]
        private Sprite spr2;

        [SerializeField]
        private Image title;

        private void Start() {
        }

        public void Turn() {
            Debug.Log("flip");
            var menu = FindObjectOfType<TiledMenu>();
            if (!menu.Turn(title.rectTransform)) {
                title.enabled = !title.enabled;
                Debug.Log("n deu");
            }
        }
    }
}