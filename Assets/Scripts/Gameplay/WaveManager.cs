using System;
using System.Collections.Generic;
using UnityEngine;


public sealed class WaveManager : ITickable
{
    // Maneja el ciclo de oleadas: spawneo, reciclado y progresion

    private readonly ZombiePool _pool;
    private readonly UpdateManager  _updateManager;
    private readonly Transform _player;
    private readonly Transform[] _spawnPoints;
    private readonly WaveSettings _settings;

    private readonly List<Zombie> _active = new List<Zombie>(64);
    private readonly Dictionary<int, Zombie> _activeByInstanceId = new Dictionary<int, Zombie>(64);

    private PlayerLogic _playerLogic;

    private int _currentWave;
    private int _pendingToSpawn;
    private float _spawnTimer;
    private bool _finished;

    public int CurrentWave => _currentWave;
    public int AliveCount => _active.Count;
    public int TotalEnemiesLeft => _active.Count + _pendingToSpawn;
    public bool IsFinished  => _finished;

    public event Action OnVictory;
    public event Action<int> OnWaveChanged;
    public event Action<int> OnEnemyCountChanged;

    public WaveManager(ZombiePool pool, UpdateManager updateManager, Transform player, Transform[] spawnPoints, WaveSettings settings)
    {
        _pool = pool;
        _updateManager = updateManager;
        _player = player;
        _spawnPoints = spawnPoints;
        _settings = settings;

        BeginWave(1);
    }

    // PlayerLogic se inyecta después de construirse para evitar dependencia circular
    public void SetPlayerLogic(PlayerLogic playerLogic)
    {
        _playerLogic = playerLogic;
    }

    public void Tick(float deltaTime)
    {
        if (_finished) return;

        SpawnPending(deltaTime);
        RecycleFinished();

        if (_pendingToSpawn == 0 && _active.Count == 0)
        {
            if (_currentWave >= _settings.TotalWaves)
            {
                _finished = true;
                Debug.Log("[WaveManager] Victoria");
                OnVictory?.Invoke();
            }
            else
            {
                BeginWave(_currentWave + 1);
            }
        }
    }

    public void TryKill(GameObject go)
    {
        // Subimos la jerarquía por si el proyectil impactó un child del zombie
        Transform t = go.transform;
        while (t != null)
        {
            if (_activeByInstanceId.TryGetValue(t.gameObject.GetInstanceID(), out Zombie zombie))
            {
                zombie.Kill();
                return;
            }
            t = t.parent;
        }
    }

    private void BeginWave(int wave)
    {
        _currentWave = wave;
        _pendingToSpawn = _settings.BaseZombies + (wave - 1) * _settings.ZombiesPerWaveIncrement;
        _spawnTimer = 0f;
        Debug.Log($"[WaveManager] Wave {wave}/{_settings.TotalWaves}: {_pendingToSpawn} zombies");
        OnWaveChanged?.Invoke(_currentWave);
        OnEnemyCountChanged?.Invoke(_active.Count + _pendingToSpawn);
    }

    private void SpawnPending(float deltaTime)
    {
        if (_pendingToSpawn <= 0) return;

        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f) return;

        _spawnTimer = _settings.SpawnInterval;

        float speed = _settings.ZombieSpeed + (_currentWave - 1) * _settings.SpeedIncrement;
        Zombie zombie = _pool.Get();
        zombie.Spawn(NextSpawnPosition(), _player, speed, _settings.ReachRadius);

        _updateManager.Register(zombie);
        _active.Add(zombie);
        _activeByInstanceId[zombie.GameObject.GetInstanceID()] = zombie;
        _pendingToSpawn--;
        OnEnemyCountChanged?.Invoke(_active.Count + _pendingToSpawn);
    }

    private void RecycleFinished()
    {
        bool anyRecycled = false;

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            Zombie zombie = _active[i];
            if (zombie.Alive) continue;

            if (zombie.ReachedTarget)
            {
                _playerLogic?.TakeDamage(_settings.ZombieDamage);
            }

            _activeByInstanceId.Remove(zombie.GameObject.GetInstanceID());
            _updateManager.Unregister(zombie);
            _pool.Return(zombie);
            _active.RemoveAt(i);
            anyRecycled = true;
        }

        if (anyRecycled)
        {
            OnEnemyCountChanged?.Invoke(_active.Count + _pendingToSpawn);
        }
    }

    private Vector3 NextSpawnPosition()
    {
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)].position;
        }

        float   angle  = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _settings.FallbackSpawnRadius;
        return _player.position + offset;
    }
}
