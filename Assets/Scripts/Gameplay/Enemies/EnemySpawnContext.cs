using UnityEngine;

// Parámetros de spawn de un enemigo. Es struct para poder sumarle modificadores
// de dificultad sin cambiarle la firma a todos los IEnemy.
public readonly struct EnemySpawnContext
{
    public readonly Vector3 Position;
    public readonly Transform Target;
    public readonly EnemyData Data;

    // Velocidad extra que aporta la oleada actual.
    public readonly float SpeedBonus;

    public EnemySpawnContext(Vector3 position, Transform target, EnemyData data, float speedBonus)
    {
        Position = position;
        Target = target;
        Data = data;
        SpeedBonus = speedBonus;
    }
}
