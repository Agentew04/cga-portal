using System.Collections;
using UnityEngine;

namespace PortalGame.World
{
    /// <summary>
    /// Script que controla o Emancipation Grill (Fizzler) do jogo.
    /// </summary>
    public class Fizzler : MonoBehaviour
    {
        [field: Header("Configuracoes")]
        [field: SerializeField]
        public bool IsActive { get; set; }

        [Header("Referencias")]
        [SerializeField]
        private BoxCollider trigger;

        [SerializeField]
        private MeshRenderer visualTrigger;

        [Header("Visual")]
        [SerializeField]
        private float visibleTransparency = 0.53f;

        [SerializeField]
        private float invisibleTransparency = 10.0f;

        [SerializeField]
        private float visibleNormalStrength = 0.38f;

        [SerializeField]
        private float invisibleNormalStrength = 0.0f;

        [SerializeField]
        private float animationTime = 1.0f;

        private Material triggerMaterial;
        private bool lastActiveState;

        private void OnEnable() {
            triggerMaterial = visualTrigger.material;
            lastActiveState = IsActive;
        }

        private void Update() {
            trigger.gameObject.SetActive(IsActive);

            if (lastActiveState != IsActive) {
                if (IsActive) {
                    StartCoroutine(AnimateEnable());
                } else {
                    StartCoroutine(AnimateDisable());
                }
                lastActiveState = IsActive;
            }
        }

        public void OnPlayerEnter(Collider player, bool entering) {
            if (!entering) {
                return;
            }

            if(!trigger.gameObject.activeSelf || !trigger.enabled) {
                return;
            }

            if (!IsActive) {
                return;
            }

            if(player.CompareTag("Player")) {
                Player.Player plr = player.GetComponentInChildren<Player.Player>();
                plr.ClearPortals();
            }
        }

        private IEnumerator AnimateDisable() {
            visualTrigger.gameObject.SetActive(true);
            float endAnimTime = Time.time + animationTime;
            while (Time.time <= endAnimTime) {
                float t = 1.0f - (endAnimTime - Time.time) / animationTime;
                triggerMaterial.SetFloat("_Transparency", Mathf.Lerp(visibleTransparency, invisibleTransparency, t));
                triggerMaterial.SetFloat("_NormalStrength", Mathf.Lerp(visibleNormalStrength, invisibleNormalStrength, t));
                yield return null;
            }
            triggerMaterial.SetFloat("_Transparency", invisibleTransparency);
            triggerMaterial.SetFloat("_NormalStrength", invisibleNormalStrength);
            visualTrigger.gameObject.SetActive(false);
        }

        private IEnumerator AnimateEnable() {
            visualTrigger.gameObject.SetActive(true);
            float endAnimTime = Time.time + animationTime;
            while (Time.time <= endAnimTime) {
                float t = 1.0f - (endAnimTime - Time.time) / animationTime;
                triggerMaterial.SetFloat("_Transparency", Mathf.Lerp(invisibleTransparency, visibleTransparency, t));
                triggerMaterial.SetFloat("_NormalStrength", Mathf.Lerp(invisibleNormalStrength, visibleNormalStrength, t));
                yield return null;
            }
            triggerMaterial.SetFloat("_Transparency", visibleTransparency);
            triggerMaterial.SetFloat("_NormalStrength", visibleNormalStrength);
        }
    }
}
