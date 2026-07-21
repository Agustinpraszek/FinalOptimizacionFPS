using UnityEngine;

// Enemigo cuerpo a cuerpo: persigue al objetivo y se consume al alcanzarlo.
// El GameObject es solo su vista.
public sealed class Zombie : IEnemy
{
    private readonly GameObject _gameObject;
    private readonly Transform _transform;
    private readonly EnemyHealth _health = new EnemyHealth();

    private Transform _target;
    private float _speed;
    private float _reachRadius;
    private bool _active;

    public GameObject GameObject => _gameObject;
    public bool IsAlive => _active && _health.IsAlive;
    public bool IsFinished => !_active;
    public bool ReachedTarget { get; private set; }
    public int DamageToPlayer { get; private set; }
    public int Reward { get; private set; }
    public Vector3 Position => _transform.position;

    public Zombie(GameObject gameObject)
    {
        _gameObject = gameObject;
        _transform = gameObject.transform;
    }

    public void Spawn(in EnemySpawnContext context)
    {
        EnemyData data = context.Data;

        _transform.position = context.Position;
        _target = context.Target;
        _speed = data.MoveSpeed + context.SpeedBonus;
        _reachRadius = data.ReachRadius;
        DamageToPlayer = data.DamageToPlayer;
        Reward = data.Reward;

        _health.Reset(data.MaxHealth);
        ReachedTarget = false;
        _active = true;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        if (_health.TakeDamage(amount))
            _active = false; // murió con este golpe
    }

    public void Tick(float deltaTime)
    {
        if (!IsAlive || _target == null) return;

        Vector3 position = _transform.position;
        Vector3 toTarget = _target.position - position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= _reachRadius)
        {
            ReachedTarget = true;
            _active = false;
            return;
        }

        if (distance <= Mathf.Epsilon) return;

        toTarget /= distance; // normaliza sin recalcular la magnitud
        _transform.position = position + toTarget * (_speed * deltaTime);
        _transform.forward = toTarget;
    }
}
