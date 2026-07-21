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

    [Header("Progresión")]
    [Tooltip("Desde qué oleada puede aparecer.")]
    [Min(1)] public int MinWave = 1;

    [Tooltip("Peso relativo frente a los otros tipos habilitados. 0 lo desactiva.")]
    [Min(0f)] public float SpawnWeight = 1f;

    [Header("Economía")]
    [Min(0)] public int Reward = 10;

    [Header("Pool")]
    [Min(1)] public int PrewarmCount = 16;
}
