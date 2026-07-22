using System.Collections.Generic;
using UnityEngine;

// Busca por InstanceID y sube por la jerarquía, por si el collider golpeado es
// hijo de la vista registrada.
public sealed class DamageableRegistry : IDamageableRegistry
{
    private readonly Dictionary<int, IDamageable> _byInstanceId;

    public DamageableRegistry(int capacity = 64)
    {
        _byInstanceId = new Dictionary<int, IDamageable>(capacity);
    }

    public void Register(GameObject view, IDamageable target)
    {
        if (view == null || target == null) return;
        _byInstanceId[view.GetInstanceID()] = target;
    }

    public void Unregister(GameObject view)
    {
        if (view == null) return;
        _byInstanceId.Remove(view.GetInstanceID());
    }

    public bool TryResolve(GameObject hit, out IDamageable target)
    {
        target = null;
        if (hit == null) return false;

        Transform current = hit.transform;
        while (current != null)
        {
            if (_byInstanceId.TryGetValue(current.gameObject.GetInstanceID(), out target))
                return true;

            current = current.parent;
        }

        return false;
    }
}
