using UnityEngine;

// Traduce un GameObject impactado a la entidad lógica que le corresponde.
// Es el puente entre "le pegué a un collider" y "a quién le pegué", sin que los
// sistemas se referencien entre sí.
public interface IDamageableRegistry
{
    void Register(GameObject view, IDamageable target);
    void Unregister(GameObject view);
    bool TryResolve(GameObject hit, out IDamageable target);
}
