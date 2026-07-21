using System.Collections.Generic;
using UnityEngine;

// Spawnea y recicla efectos visuales, un pool por VfxData.
// Play() ignora en silencio los VfxData sin asignar: el aviso de que faltan lo da
// VfxSettings.LogMissing() una sola vez al arrancar, así la consola no se llena.
public sealed class VfxSystem : IUpdatable
{
    private readonly UpdateManager _updateManager;
    private readonly Transform _root;
    private readonly Dictionary<VfxData, Pool<VfxInstance>> _poolsByData;
    private readonly Dictionary<VfxInstance, Pool<VfxInstance>> _ownerPool;
    private readonly List<VfxInstance> _active = new List<VfxInstance>(64);

    public int ActiveCount => _active.Count;

    public VfxSystem(Transform root, UpdateManager updateManager, VfxSettings settings)
    {
        _root = root;
        _updateManager = updateManager;
        _poolsByData = new Dictionary<VfxData, Pool<VfxInstance>>(4);
        _ownerPool = new Dictionary<VfxInstance, Pool<VfxInstance>>(64);

        if (settings == null) return;

        // Prewarm de los que sí están asignados. Los que falten se saltean.
        Prewarm(settings.Impact);
        Prewarm(settings.EnemyDeath);
        Prewarm(settings.MuzzleFlash);
    }

    public void Play(VfxData data, Vector3 position, Vector3 forward)
    {
        if (data == null || !data.IsUsable) return;

        Pool<VfxInstance> pool = GetOrCreatePool(data);
        VfxInstance instance = pool.Get();
        _ownerPool[instance] = pool;

        instance.Play(position, forward, data.Duration);
        _updateManager.Register(instance);
        _active.Add(instance);
    }

    public void Tick(float deltaTime)
    {
        // Al revés para poder remover en la misma pasada.
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            VfxInstance instance = _active[i];
            if (instance.IsActive) continue;

            instance.Stop();
            _updateManager.Unregister(instance);

            if (_ownerPool.TryGetValue(instance, out Pool<VfxInstance> pool))
                pool.Return(instance);

            _active.RemoveAt(i);
        }
    }

    private void Prewarm(VfxData data)
    {
        if (data == null || !data.IsUsable) return;
        GetOrCreatePool(data);
    }

    private Pool<VfxInstance> GetOrCreatePool(VfxData data)
    {
        if (_poolsByData.TryGetValue(data, out Pool<VfxInstance> pool)) return pool;

        pool = new Pool<VfxInstance>(data.Prefab, _root, data.PrewarmCount, view => new VfxInstance(view));
        _poolsByData.Add(data, pool);
        return pool;
    }
}
