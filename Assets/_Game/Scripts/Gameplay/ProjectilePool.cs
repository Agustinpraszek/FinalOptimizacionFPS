using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Pool de proyectiles. Mismo patrón que ZombiePool: Stack LIFO,
    /// prewarm en construcción, sin Instantiate/Destroy en gameplay.
    /// </summary>
    public sealed class ProjectilePool
    {
        readonly GameObject      _prefab;
        readonly Transform       _root;
        readonly Stack<Projectile> _available;

        public ProjectilePool(GameObject prefab, Transform root, int prewarm)
        {
            _prefab    = prefab;
            _root      = root;
            _available = new Stack<Projectile>(prewarm);

            for (int i = 0; i < prewarm; i++)
                _available.Push(Create());
        }

        Projectile Create()
        {
            GameObject go = Object.Instantiate(_prefab, _root);
            go.SetActive(false);
            return new Projectile(go);
        }

        public Projectile Get()
            => _available.Count > 0 ? _available.Pop() : Create();

        public void Return(Projectile projectile)
        {
            projectile.Deactivate();
            _available.Push(projectile);
        }
    }
}
