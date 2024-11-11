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

        private readonly List<List<GameObject>> tiles = new();
        private bool isFrontRendering = true;
        private int targetFlippedTiles = 0;
        private int flippedTiles = 0;
        private Vector4 tileBounds;

        private void Start() {
            CreateTiles();

            PreHeatBackBuffer();
        }

        private void CreateTiles() {
            // esperamos 16:9 x resolucao
            float width = 16.0f * resolution;
            float height = 9.0f * resolution;
            for (int x = 0; x < width; x++) {
                List<GameObject> line = new();
                for (int y = 0; y < height; y++) {
                    // centra as coord
                    float xCoord = x - width * 0.5f;
                    float yCoord = y - height * 0.5f;

                    var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.transform.SetParent(actualCamera.transform);
                    tile.transform.SetLocalPositionAndRotation(
                        localPosition: new Vector3(xCoord + 0.5f, yCoord + 0.5f, cameraDistance), 
                        localRotation: Quaternion.identity);
                    tile.transform.localScale = Vector3.one;
                    line.Add(tile);
                    var mr = tile.GetComponent<MeshRenderer>();
                    mr.material = menuMaterial;
                    Vector2 uv1 = new(x / width, y / height);
                    Vector2 uv2 = new((x+1) / width, (y+1) / height);
                    mr.material.SetVector("_UVCoord1", uv1);
                    mr.material.SetVector("_UVCoord2", uv2);
                    mr.material.SetTexture("_Front", renderTexture1);
                    mr.material.SetTexture("_Back", renderTexture2);
                }
                tiles.Add(line);
            }

            // configure camera size
            actualCamera.orthographicSize = 9.0f * resolution / 2.0f;
            actualCamera.farClipPlane = cameraDistance + 1;
        }

        private void PreHeatBackBuffer() {
            uiCamera.targetTexture = renderTexture2;
            uiCamera.Render();
            uiCamera.targetTexture = renderTexture1;
        }

        public bool Turn(RectTransform rect = null) {
            if (flippedTiles < targetFlippedTiles) {
                Debug.Log("Ja tem uma animacao em andamento");
                return false;
            }
            if(rect == null) {
                targetFlippedTiles = tiles.Count * tiles[0].Count;
                tileBounds = new Vector4(0, 0, 16 * resolution, 9 * resolution);
            } else {
                (tileBounds, targetFlippedTiles) = CalculateTargetTiles(rect);
                Debug.Log($"Target tiles: {targetFlippedTiles}");
            }

            // swap back e front buffer
            if (isFrontRendering) {
                uiCamera.targetTexture = renderTexture1;
            } else {
                uiCamera.targetTexture = renderTexture2;
            }
            isFrontRendering.Toggle();

            // anima tiles
            Quaternion rotateAround = Quaternion.Euler(0, 180, 0);
            float xAxisDelay = tileTurnTime / (16 * resolution);
            float yAxisDelay = tileTurnTime / (9 * resolution);
            for (int i = 0; i < tiles.Count; i++) {
                for (int j = 0; j < tiles[i].Count; j++) {
                    if (i < tileBounds.x || i >= tileBounds.z || j < tileBounds.y || j >= tileBounds.w) {
                        // flipa instantaneo
                        tiles[i][j].transform.rotation = tiles[i][j].transform.rotation * rotateAround;
                    } else {
                        float delay = i * yAxisDelay + j * xAxisDelay;
                        StartCoroutine(Rotate(tiles[i][j], rotateAround, delay));
                    }
                }
            }
            return true;
        }

        private IEnumerator Rotate(GameObject obj, Quaternion rotation, float delay) {
            yield return new WaitForSeconds(delay);
            Quaternion startRotation = obj.transform.rotation;
            Quaternion endRotation = startRotation * rotation;
            float elapsedTime = 0;
            while (elapsedTime < tileTurnTime) {
                obj.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / tileTurnTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            obj.transform.rotation = endRotation;
            flippedTiles++;
            if (flippedTiles == targetFlippedTiles) {
                // acabou a animacao
                Debug.Log("Acabou a animacao");
                flippedTiles = 0;
                targetFlippedTiles = 0;
            }
        }

        private (Vector4 bounds, int total) CalculateTargetTiles(RectTransform transform) {
            var screenSize = ui.renderingDisplaySize;
            var rect = transform.rect;

            // Define o número de tiles horizontal e verticalmente com base em resolution
            int tilesX = 16 * resolution;
            int tilesY = 9 * resolution;

            // Calcula o tamanho de cada tile
            var tileWidth = screenSize.x / tilesX;
            var tileHeight = screenSize.y / tilesY;

            // no espaco 0x0 -> 1920x1080
            var screenSpaceRectBounds = transform.GetCanvasSpaceBounds(ui);

            // Calcula o número de tiles que a rect ocupa
            int minTileX = Mathf.FloorToInt(screenSpaceRectBounds.min.x / tileWidth);
            int minTileY = Mathf.FloorToInt(screenSpaceRectBounds.min.y / tileHeight);
            int maxTileX = Mathf.CeilToInt(screenSpaceRectBounds.max.x / tileWidth);
            int maxTileY = Mathf.CeilToInt(screenSpaceRectBounds.max.y / tileHeight);

            // Limita os valores para o intervalo [0, 16*resolution] e [0, 9*resolution]
            minTileX = Mathf.Clamp(minTileX, 0, tilesX-1);
            minTileY = Mathf.Clamp(minTileY, 0, tilesY-1);
            maxTileX = Mathf.Clamp(maxTileX, 0, tilesX-1);
            maxTileY = Mathf.Clamp(maxTileY, 0, tilesY-1);

            // Calcula o total de tiles que a rect ocupa
            int totalTiles = (maxTileX - minTileX + 1) * (maxTileY - minTileY + 1);


            return (new Vector4(minTileX, minTileY, maxTileX, maxTileY), totalTiles);
        }

        private void OnDestroy() {
            foreach (var line in tiles) {
                foreach (var tile in line) {
                    Destroy(tile);
                }
                line.Clear();
            }
            tiles.Clear();
        }
    }
}