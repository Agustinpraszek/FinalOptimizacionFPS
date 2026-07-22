using System.Collections.Generic;
using UnityEngine;

// Ciclo de vida de los enemigos: un pool por EnemyData, más el alta y baja en el
// registry. Es el único lugar que conoce las clases concretas de enemigo.
public sealed class EnemySpawnService
{
    private readonly Transform _root;
    private readonly IDamageableRegistry _registry;
    private readonly Dictionary<EnemyData, Pool<IEnemy>> _poolsByData;

    // Para devolver cada instancia al pool del que salió.
    private readonly Dictionary<IEnemy, Pool<IEnemy>> _ownerPool;

    public EnemySpawnService(Transform root, IDamageableRegistry registry, IReadOnlyList<EnemyData> types)
    {
        _root = root;
        _registry = registry;
        _poolsByData = new Dictionary<EnemyData, Pool<IEnemy>>(4);
        _ownerPool = new Dictionary<IEnemy, Pool<IEnemy>>(128);

        if (types == null) return;

        // Prewarm de los tipos conocidos, para no hacer Instantiate en gameplay.
        for (int i = 0; i < types.Count; i++)
        {
            if (types[i] != null) GetOrCreatePool(types[i]);
        }
    }

    public IEnemy Spawn(in EnemySpawnContext context)
    {
        Pool<IEnemy> pool = GetOrCreatePool(context.Data);

        IEnemy enemy = pool.Get();
        _ownerPool[enemy] = pool;

        enemy.Spawn(in context);
        _registry.Register(enemy.GameObject, enemy);
        return enemy;
    }

    public void Despawn(IEnemy enemy)
    {
        if (enemy == null) return;

        _registry.Unregister(enemy.GameObject);

        if (_ownerPool.TryGetValue(enemy, out Pool<IEnemy> pool))
            pool.Return(enemy);
    }

    private Pool<IEnemy> GetOrCreatePool(EnemyData data)
    {
        if (_poolsByData.TryGetValue(data, out Pool<IEnemy> pool)) return pool;

        pool = new Pool<IEnemy>(data.Prefab, _root, data.PrewarmCount, CreateEnemy);
        _poolsByData.Add(data, pool);
        return pool;
    }

    // Acá se agregan las clases concretas nuevas.
    private static IEnemy CreateEnemy(GameObject view) => new Zombie(view);
}
