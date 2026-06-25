using UnityEngine;

public sealed class Zombie : ITickable
{
    //Logica del Zombie
    // WaveManager lo saca del pool, lo spawnea y lo recicla cuando termina

    private readonly GameObject _gameObject;
    private readonly Transform _transform;

    private Transform _target;
    private float _speed;
    private float _reachRadius;
    private bool _alive;

    public GameObject GameObject => _gameObject;
    public bool Alive => _alive;
    public bool ReachedTarget { get; private set; }

    public Zombie(GameObject gameObject)
    {
        _gameObject = gameObject;
        _transform = gameObject.transform;
    }

    public void Spawn(Vector3 position, Transform target, float speed, float reachRadius)
    {
        _transform.position = position;
        _target = target;
        _speed = speed;
        _reachRadius = reachRadius;
        _alive = true;
        ReachedTarget = false;
    }

    public void Kill()
    {
        _alive = false;
    }

    public void Tick(float deltaTime)
    {
        if (!_alive || _target == null) return;

        Vector3 pos = _transform.position;
        Vector3 dir = _target.position - pos;
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist <= _reachRadius)
        {
            ReachedTarget = true;
            _alive = false;
            return;
        }

        dir /= dist; // normaliza sin recalcular magnitude
        _transform.position = pos + dir * (_speed * deltaTime);
        _transform.forward  = dir;
    }
}
