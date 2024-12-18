using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

namespace PortalGame.World {

    /// <summary>
    /// Gerenciador de niveis. Carrega, descarrega e
    /// coloca secoes de loading de acordo com o necessario
    /// </summary>
    public class LevelManager : MonoBehaviour {

        [Header("Configuracoes")]
        [SerializeField, Tooltip("Define se o objeto carrega os niveis direto ou espera uma ativacao")]
        private bool loadOnStart = true;
        [SerializeField, Tooltip("Lista das cenas dos niveis")]
        private List<SceneReference> levelScenes;

        private List<bool> scenesLoaded = new();

        [Header("Referencias")]
        [SerializeField]
        private GameObject loadingRoomPrefab;
        [SerializeField]
        private WaitRoom previousLoadingCorridor;
        [SerializeField]
        private WaitRoom nextLoadingCorridor;

        private int currentLevelIndex;
        private Level currentLevel;

        [SerializeField]
        private Player.Player player;
        public Player.Player Player => player;


        private void Start() {
            scenesLoaded = Enumerable.Repeat(false, levelScenes.Count).ToList();
            if (loadOnStart) {
                Load(0, null);
            }
        }

        public void LoadNextLevel(Action callback = null) {
            Load(currentLevelIndex+1, callback);
        }

        /// <summary>
        /// Desbloqueia controle do jogador e ativa o audio dele.
        /// </summary>
        public void UnlockGameplay() {
            player.SetLock(false);
            player.SetMute(false);
        }

        /// <summary>
        /// Trava o controle do player e desliga o <see cref="AudioListener"/> dele.
        /// </summary>
        public void LockGameplay() {
            player.SetLock(true);
            player.SetMute(true);
        }

        public void Load(int index, Action callback) {
            if(index >= levelScenes.Count) {
                Debug.LogWarning("This scene does not exist!");
                return;
            }

            if(scenesLoaded.Count <= index) {
                // completar a lista
                scenesLoaded.AddRange(Enumerable.Repeat(false, index - scenesLoaded.Count + 1));
            }
            if (scenesLoaded[index]) {
                Debug.LogWarning("This scene is already loaded!");
                return;
            }

            Debug.LogFormat("Carregando cena {0}", index);

            // descarrega cena atual
            if (currentLevel != null) {
                Debug.LogFormat("Descarregando cena anterior(era {0})", currentLevelIndex);
                int unloadIndex = currentLevel.LevelIndex;
                var unloadOp = SceneManager.UnloadSceneAsync(levelScenes[currentLevel.LevelIndex].BuildIndex);
                unloadOp.completed += (e) => {
                    scenesLoaded[unloadIndex] = false;
                };
            }

            var op = SceneManager.LoadSceneAsync(levelScenes[index].BuildIndex, LoadSceneMode.Additive);
            op.completed += (op) => {
                scenesLoaded[index] = true;

                currentLevelIndex = index;
                // procura por todos pois nivel antigo pode estar carregado ainda
                var levels = FindObjectsByType<Level>(FindObjectsSortMode.None);
                currentLevel = Array.Find(levels, (x) => x.LevelIndex == index);
                if(currentLevel == null) {
                    Debug.LogError("Scene Loaded but Level script not found! Talvez o indice esteja errado??");
                    return;
                }

                // deleta corredor antigo
                if (previousLoadingCorridor != null) {
                    Destroy(previousLoadingCorridor.gameObject);
                }


                previousLoadingCorridor = nextLoadingCorridor;

                // conecta entrada do nivel na saida do corredor anterior
                // rotaciona startDoor do nivel atual para a posicao da endDoor do corredor anterior
                var transf = previousLoadingCorridor.EndDoor.transform.rotation * Quaternion.Inverse(currentLevel.StartDoor.transform.rotation);
                currentLevel.transform.rotation = transf * currentLevel.transform.rotation;
                var leveldiffpos = previousLoadingCorridor.EndDoor.transform.position - currentLevel.StartDoor.transform.position;
                currentLevel.transform.position += leveldiffpos;

                // conecta prox corredor no final do nivel
                var finalRotation = currentLevel.EndDoor.transform.rotation * Quaternion.Euler(0, 180, 0);
                // cria corredor de loading no final do nivel
                nextLoadingCorridor = Instantiate(
                    original: loadingRoomPrefab, 
                    position: currentLevel.EndDoor.transform.position,
                    rotation: finalRotation
                ).GetComponent<WaitRoom>();

                // corrige potencial erro de posicao
                nextLoadingCorridor.transform.position += currentLevel.EndDoor.transform.position - nextLoadingCorridor.StartDoor.transform.position;

                nextLoadingCorridor.Manager = this;

                // cena fica soh com porta final
                currentLevel.StartDoor.gameObject.SetActive(false);
                // previous corridor fica com porta final
                previousLoadingCorridor.EndDoor.gameObject.SetActive(true);
                // proximo corredor fica soh com a porta final tambem
                nextLoadingCorridor.StartDoor.gameObject.SetActive(false);

                // sincroniza materiais das tampas das portas
                previousLoadingCorridor.EndDoor.BackMaterial = currentLevel.StartDoor.BackMaterial;
                nextLoadingCorridor.StartDoor.BackMaterial = currentLevel.EndDoor.BackMaterial;

                callback?.Invoke();
            };
        }
    
        public static void GoToMainMenu() {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        public void Restart() {
            // cria um resetter
            var resetter = new GameObject("Resetter").AddComponent<Resetter>();
            resetter.ResetGame();
        }
    }
}
