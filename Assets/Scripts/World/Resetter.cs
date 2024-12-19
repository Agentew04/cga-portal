using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortalGame.World {
    public class Resetter : MonoBehaviour
    {
        
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void ResetGame() {
            StartCoroutine(ResetCoro());
        }

        private IEnumerator ResetCoro() {
            var unloadOp = SceneManager.UnloadSceneAsync("Main");
            Debug.Log("Esperando unload");
            while (!unloadOp.isDone) {
                yield return null;
            }
            Debug.Log("Unloaded. Loading next");
            var op = SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);

            op.completed += (op) => {
                Debug.Log("new scene loaded");
                var levelsManagers = FindObjectsByType<LevelManager>(FindObjectsSortMode.None);
                if(levelsManagers.Length != 1) {
                    Debug.LogWarning("Tem mais de um level manager!");
                }
                LevelManager manager = levelsManagers[0];
                manager.Load(0, null);
                Destroy(gameObject);
            };
        }
    }
}
