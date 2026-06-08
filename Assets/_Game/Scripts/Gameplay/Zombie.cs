using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Clase C# plana (NO MonoBehaviour). Envuelve un GameObject poolable
    /// y mueve su Transform hacia el jugador en cada Tick.
    ///
    /// Para el proyecto final podés reemplazar el movimiento directo por NavMeshAgent
    /// (ya tenés NavMeshComponents en el proyecto) sin tocar el resto de la arquitectura.
    /// </summary>
    public sealed class Zombie : ITickable
    {
        readonly GameObject _gameObject;
        readonly Transform _transform;

        Transform _target;
        float _speed;
        float _reachRadius;
        bool _alive;

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
            if (!_alive || _target == null)
                return;

            Vector3 position = _transform.position;
            Vector3 direction = _target.position - position;
            direction.y = 0f;

            float distance = direction.magnitude;
            if (distance <= _reachRadius)
            {
                ReachedTarget = true; // El WaveManager lo recoge y lo devuelve al pool.
                _alive = false;
                return;
            }

            direction /= distance; // normalizar sin segunda sqrt
            _transform.position = position + direction * (_speed * deltaTime);
            _transform.forward = direction;
        }
    }
}
