using System.Collections.Generic;
using UnityEngine;


public sealed class ZombiePool
{
    // Pool de zombies con prewarm al construirse.

    private readonly GameObject _prefab;
    private readonly Transform _root;
    private readonly Stack<Zombie> _available;

    public ZombiePool(GameObject prefab, Transform root, int prewarm)
    {
        _prefab = prefab;
        _root = root;
        _available = new Stack<Zombie>(prewarm);

        for (int i = 0; i < prewarm; i++)
        {
            _available.Push(Create());
        }
    }

    private Zombie Create()
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
