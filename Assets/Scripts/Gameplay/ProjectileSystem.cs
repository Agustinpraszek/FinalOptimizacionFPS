using System.Collections.Generic;
using UnityEngine;


public sealed class ProjectileSystem : ITickable
{
    // Dispara proyectiles y recicla los que terminaron
    // PlayerLogic solo llama Fire(), no sabe nada del pool ni del reciclado

    private readonly ProjectilePool _pool;
    private readonly UpdateManager _updateManager;
    private readonly WaveManager _waveManager;
    private readonly List<Projectile> _active = new List<Projectile>(32);

    private readonly float _speed;
    private readonly float _radius;
    private readonly float _lifetime;

    public ProjectileSystem(ProjectilePool pool, UpdateManager updateManager, WaveManager waveManager, float speed, float radius, float lifetime)
    {
        _pool = pool;
        _updateManager = updateManager;
        _waveManager = waveManager;
        _speed = speed;
        _radius = radius;
        _lifetime = lifetime;
    }

    public void Fire(Vector3 origin, Vector3 direction)
    {
        Projectile p = _pool.Get();
        p.Spawn(origin, direction, _speed, _radius, _lifetime);
        _updateManager.Register(p);
        _active.Add(p);
    }

    public void Tick(float deltaTime)
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            Projectile p = _active[i];
            if (p.IsActive) continue;

            if (p.HitTarget != null)
            {
                _waveManager.TryKill(p.HitTarget);
            }

            _updateManager.Unregister(p);
            _pool.Return(p);
            _active.RemoveAt(i);
        }
    }
}
