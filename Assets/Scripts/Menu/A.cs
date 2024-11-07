using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PortalGame.Menu {
    public class A : MonoBehaviour {

        private void Start() {
        }

        public void Turn() {
            var menu = FindObjectOfType<TiledMenu>();
            menu.Turn();
        }
    }
}