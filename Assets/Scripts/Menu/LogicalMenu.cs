using Eflatun.SceneReference;
using PortalGame.World;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortalGame.Menu
{
    /// <summary>
    /// Classe que agrega a logica dos botoes 
    /// do menu principal.
    /// </summary>
    public class LogicalMenu : MonoBehaviour {
        [Header("Referencias")]
        [SerializeField]
        private SceneReference gameplayScene;

        [SerializeField]
        private TiledMenu tiledMenu;


        private bool isLoading = false;


        public void LoadNewGame() {
            if (isLoading) {
                Debug.Log("Ja estou carregando");
                return;
            }
            isLoading = true;

            // load scenes at level 0
            int level = 0;
            var loadOp = SceneManager.LoadSceneAsync(gameplayScene.BuildIndex, LoadSceneMode.Additive);
            loadOp.completed += (op) => {
                var roots = SceneManager.GetSceneByBuildIndex(gameplayScene.BuildIndex).GetRootGameObjects();
                var manager = roots.FindFirstOf<LevelManager>();
                manager.LockGameplay();
                manager.Load(level, () => {
                    // transicao foda para gameplay
                    Debug.Log("Aqui viria a transicao");
                    _ = StartCoroutine(GameTransition(manager.Player, () => {
                        Debug.Log("Terminei de carregar, vou pedir pra descarregar o menu");
                        isLoading = false;
                        UnloadScene(() => {
                            manager.UnlockGameplay();
                        });
                    }));
                });
            };
        }

        public void ContinueGame() {
            if (isLoading) {
                Debug.Log("Ja estou carregando");
                return;
            }
            isLoading = true;

            // get last played level
            if (!PlayerPrefs.HasKey("lastLevel")) {
                PlayerPrefs.SetInt("lastLevel", 0);
            }
            int level = PlayerPrefs.GetInt("lastLevel");

            // load scenes at level x
        }

        public static void Exit() {
            Application.Quit(0);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private IEnumerator GameTransition(Player.Player player, Action callback) {
            yield return null;
            // definir a render texture certa pra camera do player
            if (tiledMenu.IsFrontRendering) {
                player.Camera.targetTexture = tiledMenu.RenderTexture2;
            } else {
                player.Camera.targetTexture = tiledMenu.RenderTexture1;
            }

            // desbindar a camera do canvas da textura, n interferir com o player
            // cagar isso n tem problema, vamos descarregar a cena dps
            tiledMenu.ActualCamera.enabled = false;
            tiledMenu.UICamera.targetTexture = null;
            tiledMenu.UICamera.enabled = false;
            tiledMenu.SetTilesVisibility(false);
            player.Camera.Render();
            tiledMenu.SetTilesVisibility(true);
            player.Camera.targetTexture = null;
            player.Camera.enabled = false;
            // volta a mostrar os tiles
            tiledMenu.ActualCamera.enabled = true;
            tiledMenu.Turn(null, () => {
                player.Camera.enabled = true;
                callback?.Invoke();
            });
        }

        /// <summary>
        /// Deleta todos os aspectos do menu principal e descarrega a cena.
        /// </summary>
        private void UnloadScene(Action callback) {
            var op = SceneManager.UnloadSceneAsync("MainMenu");
            op.completed += (AsyncOperation obj) => {
                Debug.Log("descarreguei a cena menu principal");
                var op = Resources.UnloadUnusedAssets();
                op.completed += (AsyncOperation obj) => {
                    Debug.Log("Unloaded main menu assets");
                    callback?.Invoke();
                };
            };
        }
    }
}
