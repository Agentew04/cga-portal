using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace PortalGame.World {

    /// <summary>
    /// Classe que gerencia toda a logica do inimigo.
    /// </summary>
    public class Turret : MonoBehaviour {

        [Header("Patrulha")]
        [SerializeField]
        private bool canPatrol = true;

        [SerializeField]
        private Transform patrolCenter;

        [SerializeField]
        private float patrolRadius = 5.0f;

        [SerializeField]
        private float patrolDelay = 5.0f;

        [SerializeField]
        private Vector3 currentPatrolPosition;

        [Header("Visao")]
        [SerializeField]
        private bool canSee = true;

        [SerializeField]
        private Transform eyePosition;

        [SerializeField]
        private float fieldOfView = 90.0f;

        [SerializeField]
        private int viewRaycasts = 10;

        [SerializeField]
        private float viewDistance = 5.0f;

        [SerializeField]
        private float attackDistance = 3.0f;

        [Header("Audicao")]
        [SerializeField]
        private bool canHear = true;

        [SerializeField]
        private float hearingDistance = 5.0f;

        [SerializeField]
        private Vector3 currentHintPosition;
        private bool hasHint = false;

        [Header("Visual e Referencias")]
        [SerializeField]
        private Renderer[] eyeMesh;

        [SerializeField]
        private float defaultEyeGlow;

        [SerializeField]
        private float targetAquiredEyeGlow;


        #region Private Vars

        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private float patrolTimer;

        #endregion

        private void Start() {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable() {
            TurretManager.Instance.Register(this);
        }

        private void OnDisable() {
            TurretManager.Instance.Unregister(this);
        }

        private void OnDrawGizmos() {
            if(patrolCenter != null && canPatrol) {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(patrolCenter.position, patrolRadius);
            }

            if (canHear) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, hearingDistance);
            }

            if (canSee) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, viewDistance);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, attackDistance);
            }

            if (hasHint) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(currentHintPosition, 0.5f);
            }
        }

        /// <summary>
        /// Manda uma dica de um barulho ouvido pelo inimigo.
        /// </summary>
        public void GiveHint(Vector3 hintPosition) {
            if (!canHear) {
                return;
            }
            float distance = Vector3.Distance(transform.position, hintPosition);
            if (distance > hearingDistance) {
                Debug.Log("Dica tava muito longe");
                return;
            }
            navMeshAgent.SetDestination(hintPosition);
            currentHintPosition = hintPosition;
            Debug.Log("Nova dica: " + hintPosition);
            hasHint = true;
            //animator.SetTrigger("Deploy");
        }

        private void SeeForward() {
            var forward = eyePosition.forward;
            var halfFov = fieldOfView / 2;
            var angle = -halfFov;
            var angleStep = fieldOfView / viewRaycasts;
            Ray[] rays = new Ray[viewRaycasts];
            for (int i = 0; i < viewRaycasts; i++) {
                var rotation = Quaternion.AngleAxis(angle, eyePosition.up);
                rays[i] = new Ray(eyePosition.position, rotation * forward);
                angle += angleStep;
            }

            // fazer raycasts
            var layermask = LayerMask.GetMask("Player");
            bool seesPlayer = false;
            foreach (var ray in rays) {
                if (Physics.Raycast(ray, out var hit, viewDistance, layermask)) {
                    if (hit.collider.CompareTag("Player")) {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * viewDistance, Color.green);
                        seesPlayer = true;
                    } else {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * viewDistance, Color.red);
                    }
                } else {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * viewDistance, Color.red);
                }
            }
            IEnumerable<Material> mats = eyeMesh
                .Select(x => x.materials.First(x => x.name.Contains("TurretEye")));
            if(mats.Any(x => x == null) || !mats.Any()) {
                Debug.LogError("Material nao encontrado");
                return;
            }
            Color glowColor = Color.red * Mathf.Pow(2, seesPlayer ? targetAquiredEyeGlow : defaultEyeGlow);
            mats.ForEach(x => x.SetColor("_EmissionColor", glowColor));

            // TODO usar line renderer com emission pra simular a visao do bixo!
        }

        private void Update() {
            SeeForward();
            // va para a dica

            if (hasHint) {
                // persegue a dica
            }else if (canPatrol) {
                Patrol();
            }
        }

        private void GoToHint() {
            navMeshAgent.SetDestination(currentHintPosition);
            if (Vector3.Distance(transform.position, currentHintPosition) < 1.0f) {
                currentHintPosition = Vector3.zero;
                animator.SetTrigger("Retract");
            }
        }

        private void Patrol() {
            patrolTimer += Time.deltaTime;

            if(patrolTimer > patrolDelay) {
                patrolTimer = 0;
                // acha nova posicao de patrula dentro da area
                Debug.Log("Definindo nova posicao de patrulha");
                float alpha = Random.Range(0, 360);
                float radius = Random.Range(0, patrolRadius);
                currentPatrolPosition = patrolCenter.position + new Vector3(Mathf.Cos(alpha) * radius, 0, Mathf.Sin(alpha) * radius);
                navMeshAgent.SetDestination(currentPatrolPosition);
            }
        }

        public enum TurretState {
            /// <summary>
            /// Fica procurando posicoes aleatorias na area de patrulha
            /// </summary>
            Patrolling, 
            /// <summary>
            /// Ao ouvir uma dica, vai para a posicao e investiga a area
            /// durante um tempo
            /// </summary>
            Investigating,
            /// <summary>
            /// Ao ver o jogador, vai ate a ultima posicao vista ate atacar
            /// </summary>
            Pursuing,
            /// <summary>
            /// Quando o jogador esta em distancia de ataque, atira nele
            /// </summary>
            Attacking
        }
    }
}
