using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PortalGame.Menu {
    public class A : MonoBehaviour {

        [SerializeField]
        private List<Image> images;

        private bool b = true;
        private List<Color> colors = new();

        private void Start() {
            colors = images.Select(x => x.color).ToList();
        }

        public void Turn() {
            var menu = FindObjectOfType<TiledMenu>();
            Debug.Log("tunr");
            int i = 0;
            foreach (var image in images) {
                image.color = b ? colors[i] : new Color(0, 0, 0, 1);
                i++;
            }
            b.Toggle();
            menu.Turn();
        }
    }
}