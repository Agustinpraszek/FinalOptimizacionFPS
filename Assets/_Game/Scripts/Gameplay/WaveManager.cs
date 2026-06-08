using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Lógica de oleadas (estilo CoD Zombies). Clase C# plana, tickeada
    /// desde el UpdateManager. Sin Update() nativo ni Singletons.
    ///
    /// Flujo: spawnea N zombies espaciados -> cuando todos murieron o llegaron
    /// al jugador, arranca la siguiente oleada con más enemigos y más velocidad.
    /// </summary>
    public sealed class WaveManager : ITickable
    {
        readonly ZombiePool _pool;
        readonly UpdateManager _updateManager;
        readonly Transform _player;
        readonly Transform[] _spawnPoints;
        readonly WaveSettings _settings;

        // Lista de activos + diccionario para lookup O(1) por GameObject (disparo).
        readonly List<Zombie> _active = new List<Zombie>(64);
        readonly Dictionary<int, Zombie> _activeByInstanceId = new Dictionary<int, Zombie>(64);

        // Referencia al jugador para aplicar daño cuando un zombie llega.
        PlayerLogic _playerLogic;

        int _currentWave;
        int _pendingToSpawn;
        float _spawnTimer;

        public int CurrentWave => _currentWave;
        public int AliveCount  => _active.Count;

        public WaveManager(ZombiePool pool, UpdateManager updateManager,
            Transform player, Transform[] spawnPoints, WaveSettings settings)
        {
            _pool          = pool;
            _updateManager = updateManager;
            _player        = player;
            _spawnPoints   = spawnPoints;
            _settings      = settings;

            BeginWave(1);
        }

        /// <summary>
        /// Inyectado desde GameBootstrap una vez creado el PlayerLogic.
        /// Evita dependencia circular en los constructores.
        /// </summary>
        public void SetPlayerLogic(PlayerLogic playerLogic)
            => _playerLogic = playerLogic;

        // ---- Tick -------------------------------------------------------

        public void Tick(float deltaTime)
        {
            SpawnPending(deltaTime);
            RecycleFinished();

            if (_pendingToSpawn == 0 && _active.Count == 0)
                BeginWave(_currentWave + 1);
        }

        // ---- API pública ------------------------------------------------

        /// <summary>Llamado desde PlayerLogic al disparar (raycast hit).</summary>
        public void TryKill(GameObject go)
        {
            if (_activeByInstanceId.TryGetValue(go.GetInstanceID(), out Zombie zombie))
                zombie.Kill();
        }

        // ---- Oleadas ----------------------------------------------------

        void BeginWave(int wave)
        {
            _currentWave    = wave;
            _pendingToSpawn = _settings.BaseZombies + (wave - 1) * _settings.ZombiesPerWaveIncrement;
            _spawnTimer     = 0f;
            Debug.Log($"[WaveManager] Oleada {wave}: {_pendingToSpawn} zombies");
        }

        void SpawnPending(float deltaTime)
        {
            if (_pendingToSpawn <= 0) return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f)  return;

            _spawnTimer = _settings.SpawnInterval;

            // La velocidad escala levemente con la oleada para aumentar dificultad.
            float speed = _settings.ZombieSpeed + (_currentWave - 1) * _settings.SpeedIncrement;

            Zombie zombie = _pool.Get();
            zombie.Spawn(NextSpawnPosition(), _player, speed, _settings.ReachRadius);
            _updateManager.Register(zombie);
            _active.Add(zombie);
            _activeByInstanceId[zombie.GameObject.GetInstanceID()] = zombie;
            _pendingToSpawn--;
        }

        void RecycleFinished()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Zombie zombie = _active[i];
                if (zombie.Alive) continue;

                // Si llegó al jugador (no fue baleado), aplica daño.
                if (zombie.ReachedTarget)
                    _playerLogic?.TakeDamage(_settings.ZombieDamage);

                _activeByInstanceId.Remove(zombie.GameObject.GetInstanceID());
                _updateManager.Unregister(zombie);
                _pool.Return(zombie);
                _active.RemoveAt(i);
            }
        }

        Vector3 NextSpawnPosition()
        {
            if (_spawnPoints != null && _spawnPoints.Length > 0)
                return _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

            float angle  = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle))
                             * _settings.FallbackSpawnRadius;
            return _player.position + offset;
        }
    }
}
