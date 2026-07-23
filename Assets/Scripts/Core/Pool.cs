using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Pool genérico para cualquier entidad con vista de Unity. Un tipo nuevo no
// necesita otro pool, alcanza con pasarle su factory.
// Instantiate solo en el prewarm o si se queda corto. Nunca hay Destroy en gameplay.
public sealed class Pool<T> where T : class, IPooledView
{
    private readonly GameObject _prefab;
    private readonly Transform _root;
    private readonly Func<GameObject, T> _factory;
    private readonly Stack<T> _available;

    // Evita meter dos veces el mismo objeto, que dejaría a dos usuarios operando
    // sobre la misma instancia.
    private readonly HashSet<T> _pooled;

    public int AvailableCount => _available.Count;
    public int TotalCreated { get; private set; }

    public Pool(GameObject prefab, Transform root, int prewarm, Func<GameObject, T> factory)
    {
        _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _root = root;

        int capacity = Mathf.Max(1, prewarm);
        _available = new Stack<T>(capacity);
        _pooled = new HashSet<T>();

        for (int i = 0; i < capacity; i++)
            Release(Create());
    }

    public T Get()
    {
        T item = _available.Count > 0 ? _available.Pop() : Create();
        _pooled.Remove(item);
        item.GameObject.SetActive(true);
        return item;
    }

    public void Return(T item)
    {
        if (item == null) return;
        Release(item);
    }

    private void Release(T item)
    {
        if (!_pooled.Add(item)) return; // ya estaba devuelto

        item.GameObject.SetActive(false);
        _available.Push(item);
    }

    private T Create()
    {
        GameObject instance = Object.Instantiate(_prefab, _root);
        instance.SetActive(false);
        TotalCreated++;
        return _factory(instance);
    }
}
