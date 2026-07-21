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
    private WeaponSystem _weaponSystem;
    private WeaponViewBinder _weaponView;
    private PlayerLogic _playerLogic;
    private EconomyService _economy;
    private ShopSystem _shop;
    private VfxSystem _vfx;
    private HudPresenter _hud;

    private void Awake()
    {
        if (!Validate()) return;

        _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

        var registry = new DamageableRegistry();

        _gameFlow = new GameFlowSystem(_updateManager);
        _updateManager.Register(_gameFlow, UpdateChannel.Always);

        _economy = new EconomyService(_config.StartingMoney);

        // Los VFX son opcionales: acá solo se avisa qué falta, no se aborta.
        _config.Vfx?.LogMissing(this);
        _vfx = new VfxSystem(new GameObject("VfxPool").transform, _updateManager, _config.Vfx);

        BuildEnemySide(registry);
        BuildCombatSide(registry);
        BuildShop();
        BuildHud();
        WireEvents();

        // El orden de registro es el orden de ejecución del loop.
        _updateManager.Register(_playerLogic);
        _updateManager.Register(_weaponSystem);
        _updateManager.Register(_waveManager);
        _updateManager.Register(_projectileSystem);
        _updateManager.Register(_vfx);
        if (_shop != null) _updateManager.Register(_shop);

        _waveManager.Begin();
        _hud?.SetHealth(_playerLogic.Health);
        _hud?.SetMoney(_economy.Balance);
        _hud?.SetWeapon(_weaponSystem.CurrentWeapon.DisplayName);
        _weaponView.Show(_weaponSystem.CurrentWeapon);
    }

    // Punto único de reparto de lo que deja un enemigo al morir.
    // En la fase de VFX acá se suma el spawn de la partícula de muerte.
    private void HandleEnemyKilled(EnemyKillInfo info)
    {
        _economy.Add(info.Reward);
        _vfx.Play(_config.Vfx?.EnemyDeath, info.Position, Vector3.up);
    }

    private void HandleWeaponChanged(WeaponData weapon)
    {
        _hud?.SetWeapon(weapon.DisplayName);
        _weaponView.Show(weapon);
    }

    private void HandleProjectileImpact(Vector3 point, Vector3 normal)
    {
        _vfx.Play(_config.Vfx?.Impact, point, normal);
    }

    private void HandleShotFired(Vector3 origin, Vector3 direction)
    {
        _vfx.Play(_config.Vfx?.MuzzleFlash, origin, direction);
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
            _config.EnemyTypes);
    }

    private void BuildCombatSide(IDamageableRegistry registry)
    {
        PlayerSettings settings = _config.Player;

        ProjectileSettings projectiles = _config.Projectiles;

        Transform projectileRoot = new GameObject("ProjectilePool").transform;
        var projectilePool = new Pool<Projectile>(
            projectiles.Prefab,
            projectileRoot,
            projectiles.PrewarmCount,
            view => new Projectile(view));

        _projectileSystem = new ProjectileSystem(projectilePool, _updateManager, registry);

        PlayerReferences player = _sceneReferences.Player;

        _weaponSystem = new WeaponSystem(
            player.ShootPoint,
            player.CameraPivot,
            _projectileSystem,
            _config.Weapons,
            projectiles.HitMask,
            projectiles.Radius);

        _weaponView = new WeaponViewBinder(player.WeaponPivot, _config.Weapons);

        _playerLogic = new PlayerLogic(player.Body, player.CameraPivot, player.Rigidbody, settings);

        // El jugador también es dañable, así cualquier fuente de daño futura lo
        // resuelve sin acoplarse a PlayerLogic.
        registry.Register(player.Body.gameObject, _playerLogic);
    }

    private void BuildShop()
    {
        BuyStationSetup[] stations = _sceneReferences.BuyStations;
        if (stations == null || stations.Length == 0) return;

        // Los puestos mal armados se avisan una vez y se ignoran, en vez de
        // abortar el arranque del juego.
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] == null || !stations[i].IsValid)
            {
                Debug.LogWarning(
                    $"[Shop] SceneReferences > Buy Stations [{i}] está incompleto " +
                    "(falta el Collider o el Purchase). Ese puesto no va a funcionar.", this);
            }
        }

        var context = new ShopContext(_weaponSystem, _playerLogic);
        _shop = new ShopSystem(
            _sceneReferences.Player.CameraPivot,
            stations,
            _economy,
            context,
            _config.Shop ?? new ShopSettings());
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
        _waveManager.OnEnemyKilled += HandleEnemyKilled;
        _waveManager.OnVictory += _gameFlow.EndGame;
        _playerLogic.OnDeath += _gameFlow.EndGame;
        _projectileSystem.OnImpact += HandleProjectileImpact;
        _weaponSystem.OnShotFired += HandleShotFired;

        if (_hud == null) return;

        _waveManager.OnWaveChanged += _hud.SetWave;
        _waveManager.OnEnemyCountChanged += _hud.SetEnemyCount;
        _waveManager.OnVictory += _hud.ShowVictory;
        _playerLogic.OnHealthChanged += _hud.SetHealth;
        _playerLogic.OnDeath += _hud.ShowDefeat;
        _economy.OnBalanceChanged += _hud.SetMoney;
        _weaponSystem.OnWeaponChanged += HandleWeaponChanged;

        if (_shop != null) _shop.OnPromptChanged += _hud.SetShopPrompt;
    }

    private void OnDestroy()
    {
        if (_waveManager == null || _playerLogic == null) return;

        _waveManager.OnPlayerReached -= _playerLogic.TakeDamage;
        _waveManager.OnEnemyKilled -= HandleEnemyKilled;
        _waveManager.OnVictory -= _gameFlow.EndGame;
        _playerLogic.OnDeath -= _gameFlow.EndGame;
        _projectileSystem.OnImpact -= HandleProjectileImpact;
        _weaponSystem.OnShotFired -= HandleShotFired;

        if (_hud == null) return;

        _waveManager.OnWaveChanged -= _hud.SetWave;
        _waveManager.OnEnemyCountChanged -= _hud.SetEnemyCount;
        _waveManager.OnVictory -= _hud.ShowVictory;
        _playerLogic.OnHealthChanged -= _hud.SetHealth;
        _playerLogic.OnDeath -= _hud.ShowDefeat;
        _economy.OnBalanceChanged -= _hud.SetMoney;
        _weaponSystem.OnWeaponChanged -= HandleWeaponChanged;

        if (_shop != null) _shop.OnPromptChanged -= _hud.SetShopPrompt;
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
