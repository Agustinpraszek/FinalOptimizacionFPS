using UnityEngine;
using Game.Gameplay;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #2 (de 5 permitidos). Composition Root.
    /// Único punto con Awake(): crea y cablea todo el grafo de objetos.
    /// Inyección por constructor — sin Singletons, sin FindObjectOfType.
    ///
    /// Campos expuestos: 8 de 10 permitidos.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GamePlayerBridge _playerBridge;
        [SerializeField] GameUIBridge     _uiBridge;
        [SerializeField] GameObject       _zombiePrefab;
        [SerializeField] Transform[]      _spawnPoints;

        [Header("Settings (ScriptableObjects)")]
        [SerializeField] PlayerSettings _playerSettings;
        [SerializeField] WaveSettings   _waveSettings;

        [Header("Pools")]
        [SerializeField, Min(1)] int        _zombiePrewarmCount    = 32;
        [SerializeField]         GameObject _projectilePrefab;
        [SerializeField, Min(1)] int        _projectilePrewarmCount = 20;

        // Raíces fuertes para que el GC no colecte los POCOs.
        UpdateManager    _updateManager;
        WaveManager      _waveManager;
        ProjectileSystem _projectileSystem;
        PlayerLogic      _playerLogic;

        void Awake()
        {
            if (_playerBridge == null || _playerSettings == null || _waveSettings == null)
            {
                Debug.LogError("[Bootstrap] Faltan referencias en el Inspector.");
                return;
            }

            // 1. Update manager — único Update() del juego.
            _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

            // 2. Pool de zombies.
            Transform zombieRoot = new GameObject("ZombiePool").transform;
            ZombiePool zombiePool = new ZombiePool(EnsureZombiePrefab(), zombieRoot, _zombiePrewarmCount);

            // 3. Wave manager.
            _waveManager = new WaveManager(zombiePool, _updateManager,
                _playerBridge.transform, _spawnPoints, _waveSettings);
            _updateManager.Register(_waveManager);

            // 4. Pool de proyectiles + sistema.
            Transform projRoot = new GameObject("ProjectilePool").transform;
            ProjectilePool projectilePool = new ProjectilePool(
                EnsureProjectilePrefab(), projRoot, _projectilePrewarmCount);

            _projectileSystem = new ProjectileSystem(
                projectilePool, _updateManager, _waveManager,
                _playerSettings.ProjectileSpeed,
                _playerSettings.ProjectileLifetime);
            _updateManager.Register(_projectileSystem);

            // 5. Player logic.
            _playerLogic = new PlayerLogic(
                _playerBridge.transform,
                _playerBridge.CameraPivot,
                _playerBridge.ShootPoint,
                _playerBridge.Rigidbody,
                _waveManager,
                _projectileSystem,
                _playerSettings);
            _updateManager.Register(_playerLogic);

            // 6. Inyección diferida: el WaveManager puede dañar al jugador.
            _waveManager.SetPlayerLogic(_playerLogic);

            // 7. Conectar eventos de gameplay a la UI (si está presente).
            if (_uiBridge != null)
            {
                _uiBridge.Init(_waveSettings.TotalWaves);
                _waveManager.OnWaveChanged       += _uiBridge.SetWave;
                _waveManager.OnEnemyCountChanged += _uiBridge.SetEnemyCount;
                _playerLogic.OnHealthChanged     += _uiBridge.SetHealth;

                // Los eventos ya se dispararon en los constructores antes de que
                // hubiera suscriptores, así que forzamos los valores iniciales ahora.
                _uiBridge.SetWave(_waveManager.CurrentWave);
                _uiBridge.SetEnemyCount(_waveManager.TotalEnemiesLeft);
                _uiBridge.SetHealth(_playerLogic.Health);
            }
        }

        GameObject EnsureZombiePrefab()
        {
            if (_zombiePrefab != null) return _zombiePrefab;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "ZombieTemplate";
            cube.SetActive(false);
            return cube;
        }

        GameObject EnsureProjectilePrefab()
        {
            if (_projectilePrefab != null) return _projectilePrefab;

            // Fallback: esfera pequeña como placeholder hasta tener el prefab real.
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "ProjectileTemplate";
            sphere.transform.localScale = Vector3.one * 0.15f;
            // Sin Rigidbody — el proyectil se mueve por código (sweep raycast).
            Object.Destroy(sphere.GetComponent<SphereCollider>());
            sphere.SetActive(false);
            return sphere;
        }
    }
}
