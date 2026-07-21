using UnityEngine;

// Un tipo de enemigo. Sumar una variante (rápido, tanque) es crear otro asset,
// no escribir código.
[CreateAssetMenu(menuName = "Game/Enemy Data", fileName = "EnemyData")]
public sealed class EnemyData : ScriptableObject
{
    public GameObject Prefab;

    [Header("Stats")]
    [Min(1)] public int MaxHealth = 50;
    [Min(0.1f)] public float MoveSpeed = 3.5f;
    [Min(1)] public int DamageToPlayer = 10;
    [Min(0.1f)] public float ReachRadius = 1.5f;

    [Header("Pool")]
    [Min(1)] public int PrewarmCount = 16;
}
