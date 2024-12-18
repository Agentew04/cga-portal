using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Net.NetworkInformation;
using Mono.Cecil;

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

        [Header("Investigacao")]
        [SerializeField]
        private float investigateTime = 5.0f;

        [SerializeField]
        private float investigationRadius = 5.0f;

        [Header("Ataque")]
        [SerializeField]
        private float damage = 10.0f;

        [Header("Visual e Referencias")]
        [SerializeField]
        private Renderer[] eyeMesh;

        [SerializeField]
        private float defaultEyeGlow;

        [SerializeField]
        private float targetAquiredEyeGlow;

        [SerializeField]
        private TurretState currentState;

        #region Private Vars

        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private float patrolTimer;
        private Player.Player player; // usado para aplicar dano

        private float investigateTimer;
        private Vector3 currentInvestigationHint;

        private bool seesPlayer;
        private Vector3 playerLastSeen;


        #endregion

        private void Start() {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            player = FindAnyObjectByType<Player.Player>();
        }

        private void OnEnable() {
            TurretManager.Instance.Register(this);
        }

        private void OnDisable() {
            //TurretManager.Instance.Unregister(this);
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
            seesPlayer = false;
            foreach (var ray in rays) {
                if (Physics.Raycast(ray, out var hit, viewDistance, layermask)) {
                    if (hit.collider.CompareTag("Player")) {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * viewDistance, Color.green);
                        playerLastSeen = hit.collider.transform.position;
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
            FSM();
        }

        private void FSM() {
            switch (currentState) {
                case TurretState.Patrolling:
                    Patrol();
                    break;
                case TurretState.Investigating:
                    Investigate();
                    break;
                case TurretState.Pursuing:
                    Pursue();
                    break;
                case TurretState.Attacking:
                    Attack();
                    break;
            }
        }

        private void Patrol() {
            // verifica se tem a visao do jogador
            if (seesPlayer) {
                currentState = TurretState.Pursuing;
                Debug.Log("Patrol -> Pursuing");
                return;
            }

            patrolTimer += Time.deltaTime;

            if (patrolTimer > patrolDelay) {
                patrolTimer = 0;
                // acha nova posicao de patrula dentro da area
                Debug.Log("Definindo nova posicao de patrulha");
                float alpha = Random.Range(0, 360);
                float radius = Random.Range(0, patrolRadius);
                currentPatrolPosition = patrolCenter.position + new Vector3(Mathf.Cos(alpha) * radius, 0, Mathf.Sin(alpha) * radius);
                navMeshAgent.SetDestination(currentPatrolPosition);
                Debug.Log("-> Patrolling");
            }

            if (hasHint) {
                currentState = TurretState.Investigating;
                Debug.Log("Patrol -> Investigating");
            }
        }

        private void Investigate() {
            // verifica se tem visao do jogador
            if (seesPlayer) {
                Debug.Log("Investigate -> Pursuing");
                currentState = TurretState.Pursuing;
                return;
            }

            if(Mathf.Approximately(Mathf.Abs(investigateTimer), 0) || currentHintPosition != currentInvestigationHint) {
                // primeiro frame ou a dica mudou
                // seleciona ponto aleatorio perto
                currentInvestigationHint = currentHintPosition;
                float alpha = Random.Range(0, 360);
                float radius = Random.Range(0, investigationRadius);
                var investigationPosition = currentHintPosition + new Vector3(Mathf.Cos(alpha) * radius, 0, Mathf.Sin(alpha) * radius);
                navMeshAgent.SetDestination(investigationPosition);
                Debug.Log("-> Investigating");
            }

            if (investigateTimer > investigateTime) {
                // tempo excedido
                investigateTimer = 0;
                hasHint = false;
                currentState = TurretState.Patrolling;
                Debug.Log("Investigate -> Patrolling");
                return;
            }

            investigateTimer += Time.deltaTime;
        }

        private void Pursue() {
            if (!seesPlayer) {
                // investiga a ultima posicao
                currentState = TurretState.Investigating;
                currentHintPosition = playerLastSeen;
                Debug.Log("Pursuing -> Investigating");
                return;
            }

            if(Vector3.Distance(transform.position, playerLastSeen) <= attackDistance) {
                // consegue atacar
                currentState = TurretState.Attacking;
                Debug.Log("Pursuing -> Attacking");
                return;
            }

            navMeshAgent.SetDestination(playerLastSeen);
        }

        private void Attack() {
            if (!seesPlayer) {
                // investiga a ultima posicao
                currentState = TurretState.Investigating;
                currentHintPosition = playerLastSeen;
                Debug.Log("Attack -> Investigating");
                return;
            }

            if (Vector3.Distance(transform.position, playerLastSeen) > attackDistance
                && Vector3.Distance(transform.position, playerLastSeen) <= viewDistance) {
                // ta no range de visao mas n de ataque
                currentState = TurretState.Pursuing;
                Debug.Log("Attack -> Pursuing");
                return;
            }

            // atira no jogador
            player.InflictDamage(damage * Time.deltaTime);
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
