// Todo lo que puede recibir daño. Gracias a esto el sistema de proyectiles
// no necesita conocer a los enemigos.
public interface IDamageable
{
    bool IsAlive { get; }
    void TakeDamage(int amount);
}
