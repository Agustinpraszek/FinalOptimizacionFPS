using UnityEngine;

// Datos de vuelo e impacto de un disparo. Viajan con el proyectil para que más
// adelante convivan armas con daño y velocidad distintos.
public readonly struct ProjectileConfig
{
    public readonly float Speed;
    public readonly float Radius;
    public readonly float Lifetime;
    public readonly int Damage;
    public readonly LayerMask HitMask;

    public ProjectileConfig(float speed, float radius, float lifetime, int damage, LayerMask hitMask)
    {
        Speed = speed;
        Radius = radius;
        Lifetime = lifetime;
        Damage = damage;
        HitMask = hitMask;
    }
}
