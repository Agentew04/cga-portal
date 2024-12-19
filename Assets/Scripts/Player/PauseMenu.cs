using PortalGame.World;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace PortalGame {
    /// <summary>
    /// Script que gerencia a tela de pausa
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private LevelManager levelManager;

        private bool isPaused = false;

        #region Elementos do UI

        private VisualElement root;
        private VisualElement background;
        private VisualElement sidebar;
        private VisualElement deathContainer;
        private Button mainMenuButton;
        private Button restartButton;

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
            mainMenuButton.clicked += GoToMenu;

            deathContainer = root.Q<VisualElement>("DeathContainer");
            if (!deathContainer.ClassListContains("Death-hidden")) {
                deathContainer.AddToClassList("Death-hidden");
            }
            restartButton = root.Q<Button>("RestartButton");
            restartButton.clicked += Restart;
        }

        private void OnDestroy() {
            mainMenuButton.clicked -= GoToMenu;
            restartButton.clicked -= Restart;
        }

        public void TogglePause() {
            isPaused = !isPaused;

            if (isPaused) {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            } else {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }

            if (isPaused) {
                background.RemoveFromClassList("pauseBackground-hidden");
                sidebar.RemoveFromClassList("sideBar-hidden");
            } else {
                background.AddToClassList("pauseBackground-hidden");
                sidebar.AddToClassList("sideBar-hidden");
            }
        }

        public void ShowDeathScreen() {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            deathContainer.RemoveFromClassList("Death-hidden");
        }

        private void GoToMenu() {
            background.AddToClassList("pauseBackground-hidden");
            sidebar.AddToClassList("sideBar-hidden");
            LevelManager.GoToMainMenu();
        }

        private void Restart() {
            deathContainer.AddToClassList("Death-hidden");
            levelManager.Restart();
        }
    }
}
