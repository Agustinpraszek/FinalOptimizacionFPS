using UnityEngine;
using Game.Gameplay;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #2 (de 3 permitidos). Composition Root.
    /// Único punto con Awake(): crea y CABLEA todo el grafo de objetos a mano.
    /// Inyección de dependencias por constructor — sin Singletons, sin FindObjectOfType.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Player — arrastrar el GameObject Player acá")]
        [SerializeField] GamePlayerBridge _playerBridge;

        [Header("Zombie — vacío = usa un cubo primitivo")]
        [SerializeField] GameObject _zombiePrefab;

        [Header("Spawn points — vacío = anillo alrededor del jugador")]
        [SerializeField] Transform[] _spawnPoints;

        [Header("Pool")]
        [SerializeField, Min(1)] int _prewarmCount = 32;

        [SerializeField] WaveSettings  _waveSettings  = new WaveSettings();
        [SerializeField] PlayerSettings _playerSettings = new PlayerSettings();

        // Raíces fuertes para que el GC no colecte los POCOs.
        UpdateManager _updateManager;
        WaveManager   _waveManager;
        PlayerLogic   _playerLogic;

        void Awake()
        {
            if (_playerBridge == null)
            {
                Debug.LogError("[Bootstrap] Asigná el PlayerBridge en el Inspector.");
                return;
            }

            // 1. Update manager (único Update() del juego).
            _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

            // 2. Pool de zombies.
            Transform poolRoot = new GameObject("ZombiePool").transform;
            ZombiePool pool    = new ZombiePool(EnsurePrefab(), poolRoot, _prewarmCount);

            // 3. Wave manager (necesita al jugador para seguirlo y hacerle daño).
            _waveManager = new WaveManager(pool, _updateManager,
                _playerBridge.transform, _spawnPoints, _waveSettings);
            _updateManager.Register(_waveManager);

            // 4. Player logic (necesita al wave manager para reportar hits).
            _playerLogic = new PlayerLogic(
                _playerBridge.transform,
                _playerBridge.PlayerCamera.transform,
                _playerBridge.CharacterController,
                _waveManager,
                _playerSettings);
            _updateManager.Register(_playerLogic);

            // 5. Inyección diferida: wave manager ahora puede dañar al jugador.
            _waveManager.SetPlayerLogic(_playerLogic);
        }

        GameObject EnsurePrefab()
        {
            if (_zombiePrefab != null) return _zombiePrefab;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "ZombieTemplate";
            cube.SetActive(false);
            return cube;
        }
    }
}
