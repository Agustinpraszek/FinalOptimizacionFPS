using UnityEngine;

// Punto de composición. Crea los sistemas, les inyecta las dependencias por
// constructor, define el orden de ejecución y conecta los eventos.
// Es el único lugar donde los sistemas se conocen entre sí: ninguno usa Find,
// singletons ni referencias directas.
public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SceneReferences _sceneReferences;
    [SerializeField] private GameConfig _config;

    private UpdateManager _updateManager;
    private GameFlowSystem _gameFlow;
    private WaveManager _waveManager;
    private ProjectileSystem _projectileSystem;
    private PlayerLogic _playerLogic;
    private HudPresenter _hud;

    private void Awake()
    {
        if (!Validate()) return;

        _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

        var registry = new DamageableRegistry();

        _gameFlow = new GameFlowSystem(_updateManager);
        _updateManager.Register(_gameFlow, UpdateChannel.Always);

        BuildEnemySide(registry);
        BuildCombatSide(registry);
        BuildHud();
        WireEvents();

        // El orden de registro es el orden de ejecución del loop.
        _updateManager.Register(_playerLogic);
        _updateManager.Register(_waveManager);
        _updateManager.Register(_projectileSystem);

        _waveManager.Begin();
        _hud?.SetHealth(_playerLogic.Health);
    }

    private void BuildEnemySide(IDamageableRegistry registry)
    {
        Transform enemyRoot = new GameObject("EnemyPool").transform;
        var spawnService = new EnemySpawnService(enemyRoot, registry, _config.EnemyTypes);

        _waveManager = new WaveManager(
            spawnService,
            _updateManager,
            _sceneReferences.Player.Body,
            _sceneReferences.EnemySpawnPoints,
            _config.Waves,
            _config.EnemyTypes[0]);
    }

    private void BuildCombatSide(IDamageableRegistry registry)
    {
        PlayerSettings settings = _config.Player;

        Transform projectileRoot = new GameObject("ProjectilePool").transform;
        var projectilePool = new Pool<Projectile>(
            _config.ProjectilePrefab,
            projectileRoot,
            _config.ProjectilePrewarm,
            view => new Projectile(view));

        var projectileConfig = new ProjectileConfig(
            settings.ProjectileSpeed,
            settings.ProjectileRadius,
            settings.ProjectileLifetime,
            settings.ProjectileDamage,
            settings.ProjectileHitMask);

        _projectileSystem = new ProjectileSystem(projectilePool, _updateManager, registry, in projectileConfig);

        PlayerReferences player = _sceneReferences.Player;
        _playerLogic = new PlayerLogic(
            player.Body,
            player.CameraPivot,
            player.ShootPoint,
            player.Rigidbody,
            _projectileSystem,
            settings);

        // El jugador también es dañable, así cualquier fuente de daño futura lo
        // resuelve sin acoplarse a PlayerLogic.
        registry.Register(player.Body.gameObject, _playerLogic);
    }

    private void BuildHud()
    {
        HudReferences hudRefs = _sceneReferences.Hud;
        if (hudRefs == null) return;

        _hud = new HudPresenter(hudRefs, _config.Waves.TotalWaves);
    }

    private void WireEvents()
    {
        _waveManager.OnPlayerReached += _playerLogic.TakeDamage;
        _waveManager.OnVictory += _gameFlow.EndGame;
        _playerLogic.OnDeath += _gameFlow.EndGame;

        if (_hud == null) return;

        _waveManager.OnWaveChanged += _hud.SetWave;
        _waveManager.OnEnemyCountChanged += _hud.SetEnemyCount;
        _waveManager.OnVictory += _hud.ShowVictory;
        _playerLogic.OnHealthChanged += _hud.SetHealth;
        _playerLogic.OnDeath += _hud.ShowDefeat;
    }

    private void OnDestroy()
    {
        if (_waveManager == null || _playerLogic == null) return;

        _waveManager.OnPlayerReached -= _playerLogic.TakeDamage;
        _waveManager.OnVictory -= _gameFlow.EndGame;
        _playerLogic.OnDeath -= _gameFlow.EndGame;

        if (_hud == null) return;

        _waveManager.OnWaveChanged -= _hud.SetWave;
        _waveManager.OnEnemyCountChanged -= _hud.SetEnemyCount;
        _waveManager.OnVictory -= _hud.ShowVictory;
        _playerLogic.OnHealthChanged -= _hud.SetHealth;
        _playerLogic.OnDeath -= _hud.ShowDefeat;
    }

    private bool Validate()
    {
        if (_sceneReferences == null)
        {
            Debug.LogError("[Bootstrap] Falta asignar SceneReferences.", this);
            return false;
        }

        if (_config == null)
        {
            Debug.LogError("[Bootstrap] Falta asignar GameConfig.", this);
            return false;
        }

        if (!_config.IsValid(out string configError))
        {
            Debug.LogError($"[Bootstrap] {configError}", this);
            return false;
        }

        if (!_sceneReferences.Player.IsValid(out string playerError))
        {
            Debug.LogError($"[Bootstrap] {playerError}", this);
            return false;
        }

        return true;
    }
}
