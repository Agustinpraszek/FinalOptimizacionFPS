using System;
using UnityEngine;

// Config compartida de los proyectiles. Lo que varía por arma vive en WeaponData.
// Va agrupada para no gastar cuatro campos sueltos en GameConfig.
[Serializable]
public sealed class ProjectileSettings
{
    public GameObject Prefab;

    [Min(1)] public int PrewarmCount = 64;

    [Tooltip("Capas contra las que impacta. Tiene que excluir al jugador y al arma.")]
    public LayerMask HitMask = ~0;
}
