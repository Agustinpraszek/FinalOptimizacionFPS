using System.Collections.Generic;
using UnityEngine;

// Dispara, recicla y resuelve el daño de los proyectiles.
// No conoce al WaveManager ni a los enemigos: al impactar le pregunta al registry
// quién es el objetivo y le pega vía IDamageable. Sumar entidades dañables nuevas
// no lo obliga a cambiar.
public sealed class ProjectileSystem : IUpdatable
{
    private readonly Pool<Projectile> _pool;
    private readonly UpdateManager _updateManager;
    private readonly IDamageableRegistry _registry;
    private readonly ProjectileConfig _config;
    private readonly List<Projectile> _active = new List<Projectile>(64);

    public int ActiveCount => _active.Count;

    public ProjectileSystem(
        Pool<Projectile> pool,
        UpdateManager updateManager,
        IDamageableRegistry registry,
        in ProjectileConfig config)
    {
        _pool = pool;
        _updateManager = updateManager;
        _registry = registry;
        _config = config;
    }

    public void Fire(Vector3 origin, Vector3 direction)
    {
        Projectile projectile = _pool.Get();
        projectile.Spawn(origin, direction, in _config);

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

        // TODO: spawnear el VFX de impacto en projectile.HitPoint.
    }
}
