using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class GameBootstrap : MonoBehaviour
{
    // Crea todos los objetos e inyecta sus dependencias por constructor.
    [Header("References")]
    [SerializeField] private GamePlayerBridge _playerBridge;
    [SerializeField] private GameUIBridge _uiBridge;
    [SerializeField] private GameObject _zombiePrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Animator _pistolAnimator;

    [Header("Settings")]
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private WaveSettings _waveSettings;

    [Header("Pools")]
    [SerializeField, Min(1)] private int _zombiePrewarmCount = 32;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField, Min(1)] private int _projectilePrewarmCount = 20;

    private UpdateManager _updateManager;
    private WaveManager _waveManager;
    private ProjectileSystem _projectileSystem;
    private PlayerLogic _playerLogic;

    private bool _gameOver;

    private void Awake()
    {
        if (_playerBridge == null || _playerSettings == null || _waveSettings == null)
        {
            Debug.LogError("[Bootstrap] Faltan referencias en el Inspector.");
            return;
        }

        _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

        Transform zombieRoot = new GameObject("ZombiePool").transform;
        var zombiePool = new ZombiePool(EnsureZombiePrefab(), zombieRoot, _zombiePrewarmCount);

        _waveManager = new WaveManager(zombiePool, _updateManager,
            _playerBridge.transform, _spawnPoints, _waveSettings);
        _updateManager.Register(_waveManager);

        Transform projRoot = new GameObject("ProjectilePool").transform;
        var projectilePool = new ProjectilePool(
            EnsureProjectilePrefab(), projRoot, _projectilePrewarmCount);

        _projectileSystem = new ProjectileSystem(
            projectilePool, _updateManager, _waveManager,
            _playerSettings.ProjectileSpeed,
            _playerSettings.ProjectileRadius,
            _playerSettings.ProjectileLifetime);
        _updateManager.Register(_projectileSystem);

        _playerLogic = new PlayerLogic(
            _playerBridge.transform,
            _playerBridge.CameraPivot,
            _playerBridge.ShootPoint,
            _playerBridge.Rigidbody,
            _waveManager,
            _projectileSystem,
            _playerSettings,
            _pistolAnimator);
        _updateManager.Register(_playerLogic);

        // Injectamos playerLogic en el waveManager una vez se creo
        _waveManager.SetPlayerLogic(_playerLogic);

        _waveManager.OnVictory += OnGameOver;
        _playerLogic.OnDeath += OnGameOver;

        if (_uiBridge != null)
        {
            // Suscribimos a los eventos de cambio y seteamos los valores base
            _uiBridge.Init(_waveSettings.TotalWaves);
            _waveManager.OnWaveChanged += _uiBridge.SetWave;
            _waveManager.OnEnemyCountChanged += _uiBridge.SetEnemyCount;
            _waveManager.OnVictory += _uiBridge.ShowVictory;
            _playerLogic.OnHealthChanged += _uiBridge.SetHealth;
            _playerLogic.OnDeath += _uiBridge.ShowDefeat;

            _uiBridge.SetWave(_waveManager.CurrentWave);
            _uiBridge.SetEnemyCount(_waveManager.TotalEnemiesLeft);
            _uiBridge.SetHealth(_playerLogic.Health);
        }
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (_gameOver && kb.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnGameOver()
    {
        _gameOver = true;
        _updateManager.enabled = false;
    }

    private GameObject EnsureZombiePrefab()
    {
        if (_zombiePrefab != null) return _zombiePrefab;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "ZombieTemplate";
        cube.SetActive(false);
        return cube;
    }

    private GameObject EnsureProjectilePrefab()
    {
        if (_projectilePrefab != null) return _projectilePrefab;
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "ProjectileTemplate";
        sphere.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(sphere.GetComponent<SphereCollider>());
        sphere.SetActive(false);
        return sphere;
    }
}
