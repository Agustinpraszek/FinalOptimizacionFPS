using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Configuración de oleadas. ScriptableObject: Assets > Create > Game > Wave Settings
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Wave Settings", fileName = "WaveSettings")]
    public sealed class WaveSettings : ScriptableObject
    {
        [Min(1)]     public int   TotalWaves             = 5;
        [Min(1)]     public int   BaseZombies            = 5;
        [Min(0)]     public int   ZombiesPerWaveIncrement = 3;
        [Min(0.05f)] public float SpawnInterval          = 0.75f;
        [Min(0.1f)]  public float ZombieSpeed            = 3.5f;
        [Min(0f)]    public float SpeedIncrement         = 0.3f;
        [Min(0.1f)]  public float ReachRadius            = 1.5f;
        [Min(1f)]    public float FallbackSpawnRadius    = 15f;
        [Min(1)]     public int   ZombieDamage           = 10;
    }
}
