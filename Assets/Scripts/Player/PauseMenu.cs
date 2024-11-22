using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PortalGame
{
    /// <summary>
    /// Script que gerencia a tela de pausa
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        private bool isPaused = false;

        #region Elementos do UI

        private VisualElement root;
        private VisualElement background;
        private VisualElement sidebar;
        private Button mainMenuButton;

        #endregion

        private void Start() {
            root = ui.rootVisualElement;
            background = root.Q<VisualElement>("Blur");
            if (!background.ClassListContains("pauseBackground-hidden")) {
                background.AddToClassList("pauseBackground-hidden");
            }

            sidebar = root.Q<VisualElement>("SideBar");
            if (!sidebar.ClassListContains("sideBar-hidden")) {
                sidebar.AddToClassList("sideBar-hidden");
            }

            mainMenuButton = root.Q<Button>("MainMenuButton");
            mainMenuButton.clicked += () => {
                Debug.Log("Quer ir pro menu");
            };
        }

        public void TogglePause() {
            isPaused.Toggle();

            if (isPaused) {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            } else {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }

            if (isPaused) {
                Debug.Log("Pause");
                background.RemoveFromClassList("pauseBackground-hidden");
                sidebar.RemoveFromClassList("sideBar-hidden");
            } else {
                Debug.Log("Unpause");
                background.AddToClassList("pauseBackground-hidden");
                sidebar.AddToClassList("sideBar-hidden");
            }

        }
    }
}
