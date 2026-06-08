using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Config del jugador. Serializable para exponerla desde el GameBootstrap
    /// sin gastar campos extra en el MonoBehaviour.
    /// </summary>
    [Serializable]
    public sealed class PlayerSettings
    {
        [Min(0.1f)] public float MoveSpeed = 5f;
        [Min(0.01f)] public float MouseSensitivity = 2f;
        [Min(0f)]   public float Gravity = 20f;
        [Min(1f)]   public float ShootRange = 100f;
        [Min(1)]    public int   MaxHealth = 100;
    }
}
