using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor.Rendering;
using System;

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
        [SerializeField]
        private GameObject previousLoadingCorridor;
        [SerializeField]
        private GameObject nextLoadingCorridor;

        private void Start() {
            scenesLoaded = Enumerable.Repeat(false, levelScenes.Count).ToList();
            Load(0, null);
        }

        public void LoadNextLevel(Action callback = null) {
            Load(currentLevelIndex+1, callback);
        }

        public void Load(int index, Action callback) {
            if(index >= levelScenes.Count) {
                Debug.LogWarning("This scene does not exist!");
                return;
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
                var levels = FindObjectsOfType<Level>();
                currentLevel = System.Array.Find(levels, (x) => x.LevelIndex == index);
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
