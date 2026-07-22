// Todo lo que puede recibir daño.
// Es lo que permite que el sistema de proyectiles no conozca a los enemigos.
public interface IDamageable
{
    bool IsAlive { get; }
    void TakeDamage(int amount);
}
