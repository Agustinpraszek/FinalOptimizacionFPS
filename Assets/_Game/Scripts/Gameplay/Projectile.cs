using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// POCO (sin MonoBehaviour). Representa un proyectil pooleable.
    /// Se mueve hacia adelante cada Tick y hace un sweep-raycast para detectar
    /// colisiones en el trayecto del frame, sin necesidad de Rigidbody propio.
    /// </summary>
    public sealed class Projectile : ITickable
    {
        readonly GameObject _gameObject;
        readonly Transform  _transform;

        float     _speed;
        float     _lifetime;
        bool      _active;

        public GameObject GameObject => _gameObject;
        public bool       IsActive   => _active;

        /// <summary>
        /// GameObject impactado este frame. Null si el proyectil expiró sin golpear nada.
        /// ProjectileSystem lo lee y llama TryKill antes de devolver al pool.
        /// </summary>
        public GameObject HitTarget { get; private set; }

        public Projectile(GameObject gameObject)
        {
            _gameObject = gameObject;
            _transform  = gameObject.transform;
        }

        public void Spawn(Vector3 position, Vector3 direction, float speed, float lifetime)
        {
            _transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            _speed    = speed;
            _lifetime = lifetime;
            HitTarget = null;
            _active   = true;
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
            if (_lifetime <= 0f)
            {
                Deactivate();
                return;
            }

            float   dist = _speed * deltaTime;
            Vector3 pos  = _transform.position;

            // Sweep raycast: cubre todo el trayecto del frame, sin importar la velocidad.
            if (Physics.Raycast(pos, _transform.forward, out RaycastHit hit, dist))
            {
                HitTarget           = hit.collider.gameObject;
                _transform.position = hit.point;
                _active             = false;
                _gameObject.SetActive(false);
                return;
            }

            _transform.position = pos + _transform.forward * dist;
        }
    }
}
