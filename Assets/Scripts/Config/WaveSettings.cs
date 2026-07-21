using UnityEngine;
using UnityEngine.Serialization;

// Cómo progresan las oleadas. Las stats de cada enemigo van en EnemyData; acá
// solo está lo que cambia de una wave a la otra.
[CreateAssetMenu(menuName = "Game/Wave Settings", fileName = "WaveSettings")]
public sealed class WaveSettings : ScriptableObject
{
    [Min(1)] public int TotalWaves = 5;

    [FormerlySerializedAs("BaseZombies")]
    [Min(1)] public int BaseEnemies = 5;

    [FormerlySerializedAs("ZombiesPerWaveIncrement")]
    [Min(0)] public int EnemiesPerWaveIncrement = 3;

    [Min(0.05f)] public float SpawnInterval = 0.75f;

    [Tooltip("Velocidad extra por oleada, sobre la base del EnemyData.")]
    [Min(0f)] public float SpeedIncrementPerWave = 0.3f;

    [Tooltip("Radio de spawn cuando la escena no tiene spawn points.")]
    [Min(1f)] public float FallbackSpawnRadius = 15f;
}
