using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Config de las oleadas. Clase serializable (no MonoBehaviour) para mantener
    /// el GameBootstrap con pocos campos expuestos y poder reusar/escalar la config.
    /// </summary>
    [Serializable]
    public sealed class WaveSettings
    {
        [Min(1)] public int BaseZombies = 5;
        [Min(0)] public int ZombiesPerWaveIncrement = 3;
        [Min(0.05f)] public float SpawnInterval = 0.75f;
        [Min(0.1f)] public float ZombieSpeed = 3.5f;
        [Min(0.1f)] public float ReachRadius = 1.5f;
        [Min(1f)] public float FallbackSpawnRadius = 15f;
    }
}
