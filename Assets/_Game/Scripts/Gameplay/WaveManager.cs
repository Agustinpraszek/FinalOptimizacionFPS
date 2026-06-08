using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Lógica de oleadas (estilo CoD Zombies). Clase C# plana, se "tickea"
    /// desde el UpdateManager. No usa Update() nativo ni Singletons.
    ///
    /// Flujo: spawnea N zombies espaciados en el tiempo -> cuando todos murieron
    /// o llegaron al jugador, arranca la siguiente oleada con más enemigos.
    /// </summary>
    public sealed class WaveManager : ITickable
    {
        readonly ZombiePool _pool;
        readonly UpdateManager _updateManager;
        readonly Transform _player;
        readonly Transform[] _spawnPoints;
        readonly WaveSettings _settings;

        readonly List<Zombie> _active = new List<Zombie>(64);

        int _currentWave;
        int _pendingToSpawn;
        float _spawnTimer;

        public int CurrentWave => _currentWave;
        public int AliveCount => _active.Count;

        public WaveManager(ZombiePool pool, UpdateManager updateManager,
            Transform player, Transform[] spawnPoints, WaveSettings settings)
        {
            _pool = pool;
            _updateManager = updateManager;
            _player = player;
            _spawnPoints = spawnPoints;
            _settings = settings;

            BeginWave(1);
        }

        void BeginWave(int wave)
        {
            _currentWave = wave;
            _pendingToSpawn = _settings.BaseZombies + (wave - 1) * _settings.ZombiesPerWaveIncrement;
            _spawnTimer = 0f;
            Debug.Log($"[WaveManager] Oleada {wave}: {_pendingToSpawn} zombies");
        }

        public void Tick(float deltaTime)
        {
            SpawnPending(deltaTime);
            RecycleFinished();

            // Oleada completa: no quedan por spawnear ni vivos en escena.
            if (_pendingToSpawn == 0 && _active.Count == 0)
                BeginWave(_currentWave + 1);
        }

        void SpawnPending(float deltaTime)
        {
            if (_pendingToSpawn <= 0)
                return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f)
                return;

            _spawnTimer = _settings.SpawnInterval;

            Zombie zombie = _pool.Get();
            zombie.Spawn(NextSpawnPosition(), _player, _settings.ZombieSpeed, _settings.ReachRadius);
            _updateManager.Register(zombie);
            _active.Add(zombie);
            _pendingToSpawn--;
        }

        void RecycleFinished()
        {
            // Recorrido inverso para poder remover sin romper los índices.
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Zombie zombie = _active[i];
                if (zombie.Alive)
                    continue;

                _updateManager.Unregister(zombie);
                _pool.Return(zombie);
                _active.RemoveAt(i);
            }
        }

        Vector3 NextSpawnPosition()
        {
            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                int index = Random.Range(0, _spawnPoints.Length);
                return _spawnPoints[index].position;
            }

            // Fallback sin spawn points: anillo alrededor del jugador.
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _settings.FallbackSpawnRadius;
            return _player.position + offset;
        }
    }
}
