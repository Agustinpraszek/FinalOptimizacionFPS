using UnityEngine;

// Lo que deja un enemigo al morir. Lo consumen la economía y, más adelante, el
// sistema de VFX.
public readonly struct EnemyKillInfo
{
    public readonly Vector3 Position;
    public readonly int Reward;

    public EnemyKillInfo(Vector3 position, int reward)
    {
        Position = position;
        Reward = reward;
    }
}
