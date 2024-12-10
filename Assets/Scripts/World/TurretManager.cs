using System.Collections.Generic;
using UnityEngine;

namespace PortalGame.World
{
    /// <summary>
    /// Classe que centraliza todas as torretas que podem aparecer
    /// durante o jogo.
    /// </summary>
    public class TurretManager : MonoBehaviour
    {
        private static TurretManager instance;
        public static TurretManager Instance {
            get => instance;
            set {
                if (instance != null && value != null && instance != value) {
                    Debug.LogWarning("Tentando criar uma segunda instancia de TurretManager");
                    Destroy(value);
                    return;
                }
                instance = value;
            }
        }

        private readonly List<Turret> registeredTurrets = new();

        /// <summary>
        /// Registra uma torreta na lista
        /// </summary>
        /// <param name="turret"></param>
        public void Register(Turret turret) {
            registeredTurrets.Add(turret);
        }

        /// <summary>
        /// Remove uma torreta da lista
        /// </summary>
        /// <param name="turret"></param>
        public void Unregister(Turret turret) {
            registeredTurrets.Remove(turret);
        }

        public List<Turret> GetTurrets() {
            return registeredTurrets;
        }

        public void OnEnable() {
            Instance = this;
        }

        public void OnDisable() {
            Instance = null;
        }
    }
}
