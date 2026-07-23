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

    // Posición de intención propia, aparte de _transform.position.
    // MovePosition en un kinematic recién aplica en el próximo step de física.
    // Si Tick corre varias veces entre steps y releo _transform.position, piso
    // el pedido anterior y el zombie avanza de menos. Acá la voy acumulando.
    private Vector3 _currentPosition;

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
        _currentPosition = context.Position;

        // Alineo el Rigidbody con el teleport del pool. MovePosition interpola, así
        // que sin esto el zombie arrastra desde donde murió el anterior.
        if (_rigidbody != null) _rigidbody.position = context.Position;

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

        // Si el Tick corre, la animación tiene que estar andando.
        if (_animator != null && _animator.speed == 0f)
        {
            _animator.speed = 1f;
        }

        Vector3 toTarget = _target.position - _currentPosition;
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
        _currentPosition += toTarget * (_speed * deltaTime);

        if (_rigidbody != null) _rigidbody.MovePosition(_currentPosition);
        else _transform.position = _currentPosition; // fallback si falta el Rigidbody

        _transform.forward = toTarget;
    }

    private void StopMovement()
    {
        if (_animator != null)
        {
            _animator.speed = 0f;
        }

        if (_rigidbody != null && !_rigidbody.isKinematic)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}