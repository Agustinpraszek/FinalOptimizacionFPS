using System;
using UnityEngine;

// Retroceso visual de un arma. Va agrupado para no gastar cinco campos sueltos
// en WeaponData.
[Serializable]
public sealed class WeaponRecoil
{
    [Tooltip("Grados que patea hacia arriba por disparo.")]
    [Min(0f)] public float KickUp = 5f;

    [Tooltip("Desvío lateral aleatorio por disparo, en grados.")]
    [Min(0f)] public float KickSide = 1.5f;

    [Tooltip("Tope del retroceso acumulado. Es la red de seguridad de las armas " +
             "automáticas: sin esto el arma termina mirando al cielo.")]
    [Min(0f)] public float MaxOffset = 10f;

    [Tooltip("Qué tan rápido vuelve a su lugar. Más alto es más rápido.")]
    [Min(0.1f)] public float RecoverySpeed = 9f;

    [Tooltip("Qué tan seco se siente el golpe. Más alto es más brusco.")]
    [Min(0.1f)] public float Snappiness = 22f;
}
