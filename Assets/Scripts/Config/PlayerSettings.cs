using UnityEngine;

// Datos del jugador. Todo lo del disparo se mudó a WeaponData.
[CreateAssetMenu(menuName = "Game/Player Settings", fileName = "PlayerSettings")]
public sealed class PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    [Min(0.1f)] public float MoveSpeed = 5f;
    [Min(0.01f)] public float MouseSensitivity = 2f;
    [Min(1)] public int MaxHealth = 100;
}
