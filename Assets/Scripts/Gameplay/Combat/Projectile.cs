using UnityEngine;

// Proyectil pooleado.
// Chequea impactos con un Raycast sobre el tramo que recorre en el frame, no
// con un overlap en la posición final: así no atraviesa objetos finos a alta
// velocidad y respeta la LayerMask. Antes usaba SphereCast, pero con el radio
// que manejábamos (0.1) el sweep no sumaba precisión real y salía más caro.
// No resuelve daño, solo avisa a quién le pegó.
public sealed class Projectile : IUpdatable, IPooledView
{
    private readonly GameObject _gameObject;
    private readonly Transform _transform;
    private readonly TrailRenderer _trailRenderer;
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[4];

    private ProjectileConfig _config;
    private float _remainingLifetime;
    private bool _active;

    public GameObject GameObject => _gameObject;
    public bool IsActive => _active;

    public GameObject HitTarget { get; private set; }
    public Vector3 HitPoint { get; private set; }
    public Vector3 HitNormal { get; private set; }
    public int Damage => _config.Damage;

    public Projectile(GameObject gameObject)
    {
        _gameObject = gameObject;
        _transform = gameObject.transform;
        _trailRenderer = gameObject.GetComponentInChildren<TrailRenderer>();
    }

    public void Spawn(Vector3 position, Vector3 direction, in ProjectileConfig config)
    {
        _config = config;
        _remainingLifetime = config.Lifetime;
        HitTarget = null;
        HitPoint = position;
        HitNormal = -direction;
        _active = true;

        _transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));

        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }
    }

    public void Tick(float deltaTime)
    {
        if (!_active) return;

        _remainingLifetime -= deltaTime;
        if (_remainingLifetime <= 0f)
        {
            Deactivate();
            return;
        }

        Vector3 origin = _transform.position;
        Vector3 direction = _transform.forward;
        float step = _config.Speed * deltaTime;

        if (TryHit(origin, direction, step)) return;

        _transform.position = origin + direction * step;
    }

    private bool TryHit(Vector3 origin, Vector3 direction, float step)
    {
        int count = Physics.RaycastNonAlloc(
            origin, direction, _hitBuffer, step,
            _config.HitMask, QueryTriggerInteraction.Ignore);

        if (count == 0) return false;

        // RaycastNonAlloc no ordena los resultados, hay que buscar el más cercano.
        int nearest = 0;
        for (int i = 1; i < count; i++)
        {
            if (_hitBuffer[i].distance < _hitBuffer[nearest].distance)
                nearest = i;
        }

        RaycastHit hit = _hitBuffer[nearest];
        HitTarget = hit.collider.gameObject;
        HitPoint = hit.point;
        HitNormal = hit.normal;
        _transform.position = origin + direction * hit.distance;

        Deactivate();
        return true;
    }

    private void Deactivate()
    {
        _active = false;
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false;
        }
    }
}