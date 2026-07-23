using UnityEngine;

// Enemigo cuerpo a cuerpo: persigue al objetivo y se consume al alcanzarlo.
// El GameObject es solo su vista.
public sealed class Zombie : IEnemy
{
    private readonly GameObject _gameObject;
    private readonly Transform _transform;
    private readonly EnemyHealth _health = new EnemyHealth();
    private readonly Rigidbody _rigidbody;
    private readonly Animator _animator;

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
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _animator = gameObject.GetComponentInChildren<Animator>();
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

        if (_animator != null) _animator.speed = 1f;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;

        if (_health.TakeDamage(amount))
        {
            _active = false; // murió con este golpe
            if (_animator != null) _animator.speed = 0f;
        }
    }

    public void Tick(float deltaTime)
    {
        if (!IsAlive || _target == null)
        {
            StopMovement();
            return;
        }

        // Aseguramos que la animación esté corriendo si el Tick se está ejecutando
        if (_animator != null && _animator.speed == 0f)
        {
            _animator.speed = 1f;
        }

        Vector3 position = _transform.position;
        Vector3 toTarget = _target.position - position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance <= _reachRadius)
        {
            ReachedTarget = true;
            _active = false;
            StopMovement();
            return;
        }

        if (distance <= Mathf.Epsilon)
        {
            StopMovement();
            return;
        }

        toTarget /= distance; // normaliza sin recalcular la magnitud
        Vector3 newPosition = position + toTarget * (_speed * deltaTime);
        _rigidbody.MovePosition(newPosition);
        _transform.forward = toTarget;
    }

    private void StopMovement()
    {
        if (_animator != null)
        {
            _animator.speed = 0f;
        }

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}