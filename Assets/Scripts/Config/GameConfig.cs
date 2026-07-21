using UnityEngine;

// Asset raíz de configuración. Existe para que GameBootstrap no acumule campos
// en el Inspector: entra todo por una sola referencia.
[CreateAssetMenu(menuName = "Game/Game Config", fileName = "GameConfig")]
public sealed class GameConfig : ScriptableObject
{
    [Header("Settings")]
    public PlayerSettings Player;
    public WaveSettings Waves;

    [Header("Entities")]
    [Tooltip("Tipos de enemigo disponibles.")]
    public EnemyData[] EnemyTypes;

    [Tooltip("Armas del juego. La primera es la inicial; el resto se compra.")]
    public WeaponData[] Weapons;

    [Header("Projectiles")]
    public ProjectileSettings Projectiles;

    [Header("VFX")]
    [Tooltip("Todos opcionales. Si faltan, el juego corre sin esos efectos.")]
    public VfxSettings Vfx;

    [Header("Economía")]
    [Min(0)] public int StartingMoney = 0;

    public bool IsValid(out string error)
    {
        if (Player == null) { error = "GameConfig.Player sin asignar."; return false; }
        if (Waves == null) { error = "GameConfig.Waves sin asignar."; return false; }

        if (Projectiles == null || Projectiles.Prefab == null)
        {
            error = "GameConfig.Projectiles.Prefab sin asignar.";
            return false;
        }

        if (EnemyTypes == null || EnemyTypes.Length == 0)
        {
            error = "GameConfig.EnemyTypes está vacío.";
            return false;
        }

        if (Weapons == null || Weapons.Length == 0)
        {
            error = "GameConfig.Weapons está vacío: el jugador necesita al menos un arma.";
            return false;
        }

        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] == null) { error = $"GameConfig.Weapons[{i}] sin asignar."; return false; }
        }

        bool anyStartingType = false;
        for (int i = 0; i < EnemyTypes.Length; i++)
        {
            EnemyData type = EnemyTypes[i];
            if (type == null) { error = $"GameConfig.EnemyTypes[{i}] sin asignar."; return false; }
            if (type.Prefab == null) { error = $"{type.name}: Prefab sin asignar."; return false; }
            if (type.MinWave <= 1 && type.SpawnWeight > 0f) anyStartingType = true;
        }

        if (!anyStartingType)
        {
            error = "Ningún EnemyData tiene MinWave 1 con peso mayor a 0: la oleada 1 no puede spawnear.";
            return false;
        }

        error = null;
        return true;
    }
}
