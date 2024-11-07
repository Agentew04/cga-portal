using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.Menu {

    /// <summary>
    /// Script que cria e gerencia o efeito
    /// de tiles na tela de menu
    /// </summary>
    public class TiledMenu : MonoBehaviour {
        [Header("Configuracoes")]
        [SerializeField]
        private int resolution = 1;

        [SerializeField]
        private float cameraDistance = 5;

        [SerializeField]
        private float tileTurnTime = 1.0f;

        [Header("Referencias")]
        [SerializeField]
        private Material menuMaterial;

        [SerializeField]
        private RenderTexture renderTexture1;

        [SerializeField]
        private RenderTexture renderTexture2;

        [SerializeField]
        private Camera uiCamera;

        [SerializeField]
        private Camera actualCamera;

        [SerializeField]
        private Canvas ui;

        private readonly List<GameObject> tiles = new();
        private bool isFrontRendering = true;

        private void Start() {
            CreateTiles();

        }

        private void CreateTiles() {
            // esperamos 16:9 x resolucao
            float width = 16.0f * resolution;
            float height = 9.0f * resolution;
            for (int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    // centra as coord
                    float xCoord = x - width * 0.5f;
                    float yCoord = y - height * 0.5f;

                    var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.transform.SetParent(actualCamera.transform);
                    tile.transform.SetLocalPositionAndRotation(
                        localPosition: new Vector3(xCoord + 0.5f, yCoord + 0.5f, cameraDistance), 
                        localRotation: Quaternion.identity);
                    tile.transform.localScale = Vector3.one;
                    tiles.Add(tile);
                    var mr = tile.GetComponent<MeshRenderer>();
                    mr.material = menuMaterial;
                    Vector2 uv1 = new(x / width, y / height);
                    Vector2 uv2 = new((x+1) / width, (y+1) / height);
                    mr.material.SetVector("_UVCoord1", uv1);
                    mr.material.SetVector("_UVCoord2", uv2);
                    mr.material.SetTexture("_Front", renderTexture1);
                    mr.material.SetTexture("_Back", renderTexture2);
                }
            }

            // configure camera size
            actualCamera.orthographicSize = 9.0f * resolution / 2.0f;
            actualCamera.farClipPlane = cameraDistance + 1;
        }

        public void Turn() {
            // swap back e front buffer
            if (isFrontRendering) {
                uiCamera.targetTexture = renderTexture1;
            } else {
                uiCamera.targetTexture = renderTexture2;
            }
            isFrontRendering.Toggle();

            // anima tiles
            foreach (var tile in tiles) {
                var t = LeanTween.rotateY(tile, 180, tileTurnTime)
                    .setEaseInOutSine();
                Debug.Log("rotating a tile");
            }
        }

        private void OnDestroy() {
            foreach (var tile in tiles) {
                Destroy(tile);
            }
            tiles.Clear();
        }
    }
}