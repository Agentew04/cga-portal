using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

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

        private int currentLevelIndex;
        private Level currentLevel;
        [SerializeField]
        private GameObject previousLoadingCorridor;
        [SerializeField]
        private GameObject nextLoadingCorridor;

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


            // descarrega cena atual
            if (currentLevel != null) {
                Debug.LogFormat("Descarregando cena anterior(era {0})", currentLevelIndex);
                SceneManager.UnloadSceneAsync(levelScenes[currentLevel.LevelIndex].BuildIndex);
            }

            var op = SceneManager.LoadSceneAsync(levelScenes[index].BuildIndex, LoadSceneMode.Additive);
            op.completed += (op) => {
                scenesLoaded[index] = true;
                if (index > 0) {
                    scenesLoaded[index - 1] = false;
                }

                currentLevelIndex = index;
                var levels = FindObjectsByType<Level>(FindObjectsSortMode.None);
                currentLevel = Array.Find(levels, (x) => x.LevelIndex == index);
                if(currentLevel == null) {
                    Debug.LogError("Level not found");
                    return;
                }

                // deleta corredor antigo
                if (previousLoadingCorridor != null) {
                    Destroy(previousLoadingCorridor);
                }

                previousLoadingCorridor = nextLoadingCorridor;
                var leveldiffpos = previousLoadingCorridor.GetComponent<WaitRoom>().EndDoorPosition.position - currentLevel.StartDoorPosition.position;
               
                currentLevel.transform.position += leveldiffpos;

                var inverse = Quaternion.AngleAxis(180, Vector3.up);
                var finalRotation = currentLevel.EndDoorPosition.rotation * inverse;
                // spawn loading corridor on the end door coordinate
                nextLoadingCorridor = Instantiate(
                    original: loadingRoomPrefab, 
                    position: currentLevel.EndDoorPosition.position, 
                    rotation: finalRotation
                );
                var nextwaitroom = nextLoadingCorridor.GetComponent<WaitRoom>();
                nextwaitroom.StartDoor = currentLevel.EndDoorPosition.GetComponent<Door>();
                nextwaitroom.Manager = this;

                callback?.Invoke();
            };
        }
    }
}
