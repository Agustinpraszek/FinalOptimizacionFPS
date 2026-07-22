using UnityEngine;

// Un efecto en escena. Se apaga solo cuando se le acaba la duración y el
// VfxSystem lo devuelve al pool.
// Tolera prefabs sin ParticleSystem: en ese caso solo prende y apaga el objeto.
public sealed class VfxInstance : IUpdatable, IPooledView
{
    private readonly GameObject _gameObject;
    private readonly Transform _transform;
    private readonly ParticleSystem _particles;

    private float _remaining;
    private bool _active;

    public GameObject GameObject => _gameObject;
    public bool IsActive => _active;

    public VfxInstance(GameObject gameObject)
    {
        _gameObject = gameObject;
        _transform = gameObject.transform;

        // Se busca una sola vez, en el prewarm. Nunca durante el gameplay.
        _particles = gameObject.GetComponentInChildren<ParticleSystem>(true);
    }

    public void Play(Vector3 position, Vector3 forward, float duration)
    {
        Quaternion rotation = forward.sqrMagnitude > Mathf.Epsilon
            ? Quaternion.LookRotation(forward)
            : Quaternion.identity;

        _transform.SetPositionAndRotation(position, rotation);
        _remaining = duration;
        _active = true;

        if (_particles == null) return;

        // Clear antes de Play para que la instancia reusada no arrastre las
        // partículas del efecto anterior.
        _particles.Clear(true);
        _particles.Play(true);
    }

    public void Stop()
    {
        _active = false;

        if (_particles != null)
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Tick(float deltaTime)
    {
        if (!_active) return;

        _remaining -= deltaTime;
        if (_remaining <= 0f) _active = false;
    }
}
