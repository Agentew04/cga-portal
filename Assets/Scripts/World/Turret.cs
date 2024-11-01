using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PortalGame.World {

    /// <summary>
    /// Classe que gerencia toda a logica do inimigo.
    /// </summary>
    public class Turret : MonoBehaviour {

        [Header("Configuracoes")]
        [SerializeField]
        private Transform patrolCenter;

        [SerializeField]
        private float patrolRadius = 5.0f;


        #region Private Vars

        private NavMeshAgent navMeshAgent;
        private Vector3 hintPosition;
        private Animator animator;

        #endregion

        private void Start() {
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Manda uma dica de um barulho ouvido pelo inimigo.
        /// </summary>
        public void GiveHint(Vector3 hintPosition) {
            navMeshAgent.CalculatePath(hintPosition, navMeshAgent.path);
            this.hintPosition = hintPosition;
            animator.SetTrigger("Deploy");
        }

        private void Update() {
            // va para a dica
            if(hintPosition != Vector3.zero) {
                navMeshAgent.SetDestination(hintPosition);
                if(Vector3.Distance(transform.position, hintPosition) < 1.0f) {
                    hintPosition = Vector3.zero;
                    animator.SetTrigger("Retract");
                }
            } else {
                // patrulhe
                if(Vector3.Distance(transform.position, patrolCenter.position) < patrolRadius) {
                    Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
                    randomDirection += patrolCenter.position;
                    NavMeshHit hit;
                    NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
                    Vector3 finalPosition = hit.position;
                    navMeshAgent.SetDestination(finalPosition);
                }
            }

        }
    }
}
