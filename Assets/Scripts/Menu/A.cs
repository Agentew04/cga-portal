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

        private void Start() {
        }

        public void Turn() {
            if(back1.sprite == spr1) {
                back1.sprite = spr2;
            } else {
                back1.sprite = spr1;
            }

            var menu = FindObjectOfType<TiledMenu>();
            menu.Turn();
        }
    }
}