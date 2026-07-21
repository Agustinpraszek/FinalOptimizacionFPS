using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Settings", fileName = "PlayerSettings")]
public sealed class PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    [Min(0.1f)] public float MoveSpeed = 5f;
    [Min(0.01f)] public float MouseSensitivity = 2f;
    [Min(1)] public int MaxHealth = 100;

    [Header("Projectile")]
    [Min(1f)] public float ProjectileSpeed = 30f;
    [Min(0.1f)] public float ProjectileLifetime = 4f;
    [Min(0.01f)] public float ProjectileRadius = 0.1f;
    [Min(1)] public int ProjectileDamage = 25;

    [Tooltip("Capas contra las que impacta el proyectil. Tiene que excluir al jugador.")]
    public LayerMask ProjectileHitMask = ~0;
}
