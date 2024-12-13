using PortalGame.World;
using UnityEngine;

namespace PortalGame
{
    public class Level3 : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Door door;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (button.IsActivated) {
                door.Open();
            } else {
                door.Close();
            }
        }
    }
}
