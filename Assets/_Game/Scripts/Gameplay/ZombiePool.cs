using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Object Pooling (consigna obligatoria). Clase C# plana.
    /// Nunca hace Instantiate/Destroy en gameplay: prewarmea al inicio y reusa.
    /// Cada GameObject poolable lleva asociado su wrapper Zombie persistente,
    /// así no creamos basura ni perdemos referencias (sin memory leaks).
    /// </summary>
    public sealed class ZombiePool
    {
        readonly GameObject _prefab;
        readonly Transform _root;
        readonly Stack<Zombie> _available;

        public ZombiePool(GameObject prefab, Transform root, int prewarm)
        {
            _prefab = prefab;
            _root = root;
            _available = new Stack<Zombie>(prewarm);

            for (int i = 0; i < prewarm; i++)
                _available.Push(Create());
        }

        Zombie Create()
        {
            GameObject instance = Object.Instantiate(_prefab, _root);
            instance.SetActive(false);
            return new Zombie(instance);
        }

        public Zombie Get()
        {
            Zombie zombie = _available.Count > 0 ? _available.Pop() : Create();
            zombie.GameObject.SetActive(true);
            return zombie;
        }

        public void Return(Zombie zombie)
        {
            zombie.GameObject.SetActive(false);
            _available.Push(zombie);
        }
    }
}
