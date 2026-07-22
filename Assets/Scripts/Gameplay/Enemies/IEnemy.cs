using UnityEngine;

// Contrato de un enemigo. Una variante nueva implementa esto y el WaveManager
// la maneja sin cambios.
public interface IEnemy : IUpdatable, IPooledView, IDamageable
{
    // Llegó al jugador. Qué hacer con eso lo decide el sistema.
    bool ReachedTarget { get; }

    int DamageToPlayer { get; }

    // Plata que deja si lo matan. No la cobra si llegó al jugador.
    int Reward { get; }

    // Terminó su ciclo (murió o llegó) y hay que reciclarlo.
    bool IsFinished { get; }

    Vector3 Position { get; }

    void Spawn(in EnemySpawnContext context);
}
