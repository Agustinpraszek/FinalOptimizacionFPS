using UnityEngine;
using Game.Gameplay;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #2 (de 3 permitidos). Composition Root.
    /// Único punto con Awake(): crea y CABLEA todo el grafo de objetos a mano.
    /// Así evitamos Singletons y referencias directas innecesarias: cada clase
    /// recibe por constructor solo lo que necesita (inyección de dependencias).
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Zombie (si queda vacío se usa un cubo primitivo)")]
        [SerializeField] GameObject _zombiePrefab;

        [Header("Objetivo (si queda vacío se crea en el origen)")]
        [SerializeField] Transform _player;

        [Header("Spawn points (opcional: vacío = anillo alrededor del jugador)")]
        [SerializeField] Transform[] _spawnPoints;

        [Header("Pooling")]
        [SerializeField, Min(1)] int _prewarmCount = 32;

        [SerializeField] WaveSettings _waveSettings = new WaveSettings();

        // Referencias guardadas para que no las recolecte el GC.
        UpdateManager _updateManager;
        WaveManager _waveManager;

        void Awake()
        {
            EnsurePlayer();
            GameObject prefab = EnsurePrefab();

            _updateManager = new GameObject("UpdateManager").AddComponent<UpdateManager>();

            Transform poolRoot = new GameObject("ZombiePool").transform;
            ZombiePool pool = new ZombiePool(prefab, poolRoot, _prewarmCount);

            _waveManager = new WaveManager(pool, _updateManager, _player, _spawnPoints, _waveSettings);
            _updateManager.Register(_waveManager);
        }

        void EnsurePlayer()
        {
            if (_player == null)
                _player = new GameObject("Player").transform;
        }

        GameObject EnsurePrefab()
        {
            if (_zombiePrefab != null)
                return _zombiePrefab;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "ZombieTemplate";
            cube.SetActive(false); // queda como plantilla; el pool instancia copias.
            return cube;
        }
    }
}
