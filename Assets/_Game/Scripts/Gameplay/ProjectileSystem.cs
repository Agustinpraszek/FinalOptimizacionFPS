using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Sistema de proyectiles. Clase C# plana, tickeada desde el UpdateManager.
    /// Responsabilidades:
    ///   - Disparar: sacar del pool, spawner, registrar en el UpdateManager.
    ///   - Reciclar: detectar proyectiles inactivos, llamar TryKill si impactaron,
    ///     desregistrar del UpdateManager y devolver al pool.
    ///
    /// PlayerLogic solo llama Fire() — no sabe nada del pool ni del reciclado.
    /// </summary>
    public sealed class ProjectileSystem : ITickable
    {
        readonly ProjectilePool    _pool;
        readonly UpdateManager     _updateManager;
        readonly WaveManager       _waveManager;
        readonly List<Projectile>  _active = new List<Projectile>(32);

        readonly float _speed;
        readonly float _lifetime;

        public ProjectileSystem(ProjectilePool pool, UpdateManager updateManager,
            WaveManager waveManager, float speed, float lifetime)
        {
            _pool          = pool;
            _updateManager = updateManager;
            _waveManager   = waveManager;
            _speed         = speed;
            _lifetime      = lifetime;
        }

        /// <summary>Llamado desde PlayerLogic cuando el jugador dispara.</summary>
        public void Fire(Vector3 origin, Vector3 direction)
        {
            Projectile p = _pool.Get();
            p.Spawn(origin, direction, _speed, _lifetime);
            _updateManager.Register(p);
            _active.Add(p);
        }

        public void Tick(float deltaTime)
        {
            // Recorremos en reversa para remover sin romper índices.
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Projectile p = _active[i];
                if (p.IsActive) continue;

                // Si golpeó algo, notificamos al WaveManager.
                if (p.HitTarget != null)
                    _waveManager.TryKill(p.HitTarget);

                _updateManager.Unregister(p);
                _pool.Return(p);
                _active.RemoveAt(i);
            }
        }
    }
}
