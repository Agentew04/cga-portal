using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;
using System.Linq;

namespace PortalGame.World {

    /// <summary>
    /// Gerenciador de niveis. Carrega, descarrega e
    /// coloca secoes de loading de acordo com o necessario
    /// </summary>
    public class LevelManager : MonoBehaviour {

        [SerializeField, Tooltip("Lista das cenas dos niveis")]
        private List<SceneReference> levelScenes;

        private List<bool> scenesLoaded = new();

        [SerializeField]
        private GameObject loadingRoomPrefab;

        private int currentLevelIndex;
        private Level currentLevel;
        private GameObject currentLoadingCorridor;

        private void Start() {
            scenesLoaded = Enumerable.Repeat(false, levelScenes.Count).ToList();
            Load(0);
        }

        public void Load(int index) {
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
                currentLevelIndex = index;
                var levels = FindObjectsOfType<Level>();
                currentLevel = System.Array.Find(levels, (x) => x.LevelIndex == index);
                if(currentLevel == null) {
                    Debug.LogError("Level not found");
                    return;
                }

                // deleta corredor antigo
                if (currentLoadingCorridor != null) {
                    Destroy(currentLoadingCorridor);
                }

                // spawn loading corridor on the end door coordinate
                currentLoadingCorridor = Instantiate(
                    original: loadingRoomPrefab, 
                    position: currentLevel.EndDoorPosition.position, 
                    rotation: currentLevel.EndDoorPosition.rotation);
            };
        }
    }
}
