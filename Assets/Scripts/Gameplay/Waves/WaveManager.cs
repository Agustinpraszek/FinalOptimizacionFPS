using System;
using System.Collections.Generic;
using UnityEngine;

// Ciclo de oleadas: spawn escalonado, reciclado y avance de wave.
// Trabaja contra IEnemy y publica eventos; no conoce clases concretas ni al jugador.
public sealed class WaveManager : IUpdatable
{
    private readonly EnemySpawnService _spawnService;
    private readonly UpdateManager _updateManager;
    private readonly Transform _player;
    private readonly Transform[] _spawnPoints;
    private readonly WaveSettings _settings;
    private readonly IReadOnlyList<EnemyData> _enemyTypes;

    private readonly List<IEnemy> _active = new List<IEnemy>(64);

    private int _currentWave;
    private int _pendingToSpawn;
    private float _spawnTimer;
    private bool _finished;

    public int CurrentWave => _currentWave;
    public int AliveCount => _active.Count;
    public int TotalEnemiesLeft => _active.Count + _pendingToSpawn;
    public bool IsFinished => _finished;

    public event Action OnVictory;
    public event Action<int> OnWaveChanged;
    public event Action<int> OnEnemyCountChanged;

    // Un enemigo llegó al jugador. Quien escuche decide el efecto.
    public event Action<int> OnPlayerReached;

    // Un enemigo murió. Lo consume la economía y después los VFX.
    public event Action<EnemyKillInfo> OnEnemyKilled;

    public WaveManager(
        EnemySpawnService spawnService,
        UpdateManager updateManager,
        Transform player,
        Transform[] spawnPoints,
        WaveSettings settings,
        IReadOnlyList<EnemyData> enemyTypes)
    {
        _spawnService = spawnService;
        _updateManager = updateManager;
        _player = player;
        _spawnPoints = spawnPoints;
        _settings = settings;
        _enemyTypes = enemyTypes;
    }

    // Va aparte del constructor para que el bootstrap pueda suscribirse antes de
    // que se emita el primer evento.
    public void Begin()
    {
        BeginWave(1);
    }

    public void Tick(float deltaTime)
    {
        if (_finished) return;

        SpawnPending(deltaTime);
        RecycleFinished();

        if (_pendingToSpawn > 0 || _active.Count > 0) return;

        if (_currentWave >= _settings.TotalWaves)
        {
            _finished = true;
            OnVictory?.Invoke();
            return;
        }

        BeginWave(_currentWave + 1);
    }

    private void BeginWave(int wave)
    {
        _currentWave = wave;
        _pendingToSpawn = _settings.BaseEnemies + (wave - 1) * _settings.EnemiesPerWaveIncrement;
        _spawnTimer = 0f;

        OnWaveChanged?.Invoke(_currentWave);
        OnEnemyCountChanged?.Invoke(TotalEnemiesLeft);
    }

    private void SpawnPending(float deltaTime)
    {
        if (_pendingToSpawn <= 0) return;

        _spawnTimer -= deltaTime;
        if (_spawnTimer > 0f) return;

        _spawnTimer = _settings.SpawnInterval;

        float speedBonus = (_currentWave - 1) * _settings.SpeedIncrementPerWave;
        var context = new EnemySpawnContext(NextSpawnPosition(), _player, PickEnemyType(), speedBonus);

        IEnemy enemy = _spawnService.Spawn(in context);
        _updateManager.Register(enemy);
        _active.Add(enemy);

        _pendingToSpawn--;
        OnEnemyCountChanged?.Invoke(TotalEnemiesLeft);
    }

    // Ruleta por peso entre los tipos ya habilitados para la oleada actual.
    // Así la dificultad progresa desde los assets, sin tocar código.
    private EnemyData PickEnemyType()
    {
        float totalWeight = 0f;
        for (int i = 0; i < _enemyTypes.Count; i++)
        {
            if (IsUnlocked(_enemyTypes[i])) totalWeight += _enemyTypes[i].SpawnWeight;
        }

        if (totalWeight <= 0f) return _enemyTypes[0];

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        for (int i = 0; i < _enemyTypes.Count; i++)
        {
            EnemyData type = _enemyTypes[i];
            if (!IsUnlocked(type)) continue;

            roll -= type.SpawnWeight;
            if (roll <= 0f) return type;
        }

        return _enemyTypes[0];
    }

    private bool IsUnlocked(EnemyData type)
    {
        return type != null && type.MinWave <= _currentWave && type.SpawnWeight > 0f;
    }

    private void RecycleFinished()
    {
        bool anyRecycled = false;

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            IEnemy enemy = _active[i];
            if (!enemy.IsFinished) continue;

            if (enemy.ReachedTarget)
                OnPlayerReached?.Invoke(enemy.DamageToPlayer);
            else
                OnEnemyKilled?.Invoke(new EnemyKillInfo(enemy.Position, enemy.Reward));

            _updateManager.Unregister(enemy);
            _spawnService.Despawn(enemy);
            _active.RemoveAt(i);
            anyRecycled = true;
        }

        if (anyRecycled) OnEnemyCountChanged?.Invoke(TotalEnemiesLeft);
    }

    private Vector3 NextSpawnPosition()
    {
        if (_spawnPoints != null && _spawnPoints.Length > 0)
            return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)].position;

        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _settings.FallbackSpawnRadius;
        return _player.position + offset;
    }
}
