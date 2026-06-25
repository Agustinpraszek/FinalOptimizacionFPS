using UnityEngine;

public sealed class Projectile : ITickable
{
    private readonly GameObject _gameObject;
    private readonly Transform  _transform;
    private readonly Collider[] _hitBuffer = new Collider[4];

    private float _speed;
    private float _radius;
    private float _lifetime;
    private bool _active;

    public GameObject GameObject => _gameObject;
    public bool IsActive => _active;
    public GameObject HitTarget  { get; private set; }

    public Projectile(GameObject gameObject)
    {
        _gameObject = gameObject;
        _transform  = gameObject.transform;
    }

    public void Spawn(Vector3 position, Vector3 direction, float speed, float radius, float lifetime)
    {
        _transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
        _speed = speed;
        _radius = radius;
        _lifetime = lifetime;
        HitTarget = null;
        _active = true;
        _gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _active = false;
        _gameObject.SetActive(false);
    }

    public void Tick(float deltaTime)
    {
        if (!_active) return;

        _lifetime -= deltaTime;
        if (_lifetime <= 0f) { Deactivate(); return; }

        _transform.position += _transform.forward * (_speed * deltaTime);

        int count = Physics.OverlapSphereNonAlloc(_transform.position, _radius, _hitBuffer);

        for (int i = 0; i < count; i++)
        {
            GameObject hit = _hitBuffer[i].gameObject;
            if (hit == _gameObject) continue;

            HitTarget = hit;
            _active = false;
            _gameObject.SetActive(false);
            return;
        }
    }
}
