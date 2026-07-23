using System;
using System.Collections.Generic;
using UnityEngine;

// Dispara, recicla y resuelve el daño de los proyectiles.
// No conoce al WaveManager ni a los enemigos. Al impactar le pregunta al registry
// quién es el objetivo y le pega vía IDamageable, así sumar entidades dañables
// nuevas no lo obliga a cambiar.
// La config llega por disparo, así conviven armas con daño y velocidad distintos.
public sealed class ProjectileSystem : IUpdatable
{
    private readonly Pool<Projectile> _pool;
    private readonly UpdateManager _updateManager;
    private readonly IDamageableRegistry _registry;
    private readonly List<Projectile> _active = new List<Projectile>(64);

    public int ActiveCount => _active.Count;

    // Punto y normal del impacto. Lo consume el VFX. El sistema no sabe quién escucha.
    public event Action<Vector3, Vector3> OnImpact;

    public ProjectileSystem(Pool<Projectile> pool, UpdateManager updateManager, IDamageableRegistry registry)
    {
        _pool = pool;
        _updateManager = updateManager;
        _registry = registry;
    }

    public void Fire(Vector3 origin, Vector3 direction, in ProjectileConfig config)
    {
        Projectile projectile = _pool.Get();
        projectile.Spawn(origin, direction, in config);

        _updateManager.Register(projectile);
        _active.Add(projectile);
    }

    public void Tick(float deltaTime)
    {
        // Al revés para poder remover en la misma pasada.
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            Projectile projectile = _active[i];
            if (projectile.IsActive) continue;

            ResolveImpact(projectile);

            _updateManager.Unregister(projectile);
            _pool.Return(projectile);
            _active.RemoveAt(i);
        }
    }

    private void ResolveImpact(Projectile projectile)
    {
        if (projectile.HitTarget == null) return; // expiró por lifetime

        if (_registry.TryResolve(projectile.HitTarget, out IDamageable target))
            target.TakeDamage(projectile.Damage);

        OnImpact?.Invoke(projectile.HitPoint, projectile.HitNormal);
    }
}
