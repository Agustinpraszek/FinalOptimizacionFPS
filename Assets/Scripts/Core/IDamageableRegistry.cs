using UnityEngine;

// Traduce el GameObject impactado a su entidad lógica. El puente entre
// "le pegué a un collider" y "a quién le pegué", sin acoplar sistemas.
public interface IDamageableRegistry
{
    void Register(GameObject view, IDamageable target);
    void Unregister(GameObject view);
    bool TryResolve(GameObject hit, out IDamageable target);
}
