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
    [Tooltip("Tipos de enemigo disponibles. Hoy se usa el primero.")]
    public EnemyData[] EnemyTypes;

    [Header("Projectiles")]
    public GameObject ProjectilePrefab;
    [Min(1)] public int ProjectilePrewarm = 32;

    public bool IsValid(out string error)
    {
        if (Player == null) { error = "GameConfig.Player sin asignar."; return false; }
        if (Waves == null) { error = "GameConfig.Waves sin asignar."; return false; }
        if (ProjectilePrefab == null) { error = "GameConfig.ProjectilePrefab sin asignar."; return false; }
        if (EnemyTypes == null || EnemyTypes.Length == 0 || EnemyTypes[0] == null)
        {
            error = "GameConfig.EnemyTypes vacío o con elementos nulos.";
            return false;
        }
        if (EnemyTypes[0].Prefab == null) { error = "EnemyData.Prefab sin asignar."; return false; }

        error = null;
        return true;
    }
}
