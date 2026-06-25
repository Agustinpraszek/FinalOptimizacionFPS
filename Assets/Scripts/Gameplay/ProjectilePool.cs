using System.Collections.Generic;
using UnityEngine;

public sealed class ProjectilePool
{

    private readonly GameObject _prefab;
    private readonly Transform _root;
    private readonly Stack<Projectile> _available;

    public ProjectilePool(GameObject prefab, Transform root, int prewarm)
    {
        _prefab = prefab;
        _root = root;
        _available = new Stack<Projectile>(prewarm);

        for (int i = 0; i < prewarm; i++)
        {
            _available.Push(Create());
        }
    }

    private Projectile Create()
    {
        GameObject go = Object.Instantiate(_prefab, _root);
        go.SetActive(false);
        return new Projectile(go);
    }

    public Projectile Get()
    {
        return _available.Count > 0 ? _available.Pop() : Create();
    }

    public void Return(Projectile projectile)
    {
        projectile.Deactivate();
        _available.Push(projectile);
    }
}
