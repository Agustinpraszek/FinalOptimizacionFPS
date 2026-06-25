using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// POCO (sin MonoBehaviour). Lógica completa de primera persona:
    /// movimiento, mouse-look y disparo.
    /// Tickeado desde el UpdateManager — jamás usa Update() nativo.
    ///
    /// Disparo delegado a ProjectileSystem: PlayerLogic solo llama Fire()
    /// con origen y dirección. No sabe nada de pools ni de reciclado.
    /// </summary>
    public sealed class PlayerLogic : ITickable
    {
        readonly Transform         _body;
        readonly Transform         _cameraPivot;
        readonly Transform         _shootPoint;
        readonly Rigidbody         _rb;
        readonly WaveManager       _waveManager;
        readonly ProjectileSystem  _projectileSystem;

        readonly float _moveSpeed;
        readonly float _mouseSensitivity;

        float _pitch;
        float _yaw;
        int   _health;

        public int  Health => _health;
        public bool IsDead => _health <= 0;

        public event Action<int> OnHealthChanged;

        public PlayerLogic(Transform body, Transform cameraPivot, Transform shootPoint,
            Rigidbody rb, WaveManager waveManager,
            ProjectileSystem projectileSystem, PlayerSettings settings)
        {
            _body             = body;
            _cameraPivot      = cameraPivot;
            _shootPoint       = shootPoint;
            _rb               = rb;
            _waveManager      = waveManager;
            _projectileSystem = projectileSystem;
            _moveSpeed        = settings.MoveSpeed;
            _mouseSensitivity = settings.MouseSensitivity;
            _health           = settings.MaxHealth;

            _rb.freezeRotation = true;
            _yaw = _body.eulerAngles.y;
        }

        public void Tick(float deltaTime)
        {
            if (IsDead) return;

            HandleLook();
            HandleMove(deltaTime);
            HandleShoot();
        }

        public void TakeDamage(int amount)
        {
            _health = Mathf.Max(0, _health - amount);
            OnHealthChanged?.Invoke(_health);
            if (IsDead)
                Debug.Log("[Player] ¡Jugador muerto!");
        }

        // ---- Look -------------------------------------------------------

        void HandleLook()
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * (_mouseSensitivity * 0.1f);

            _yaw += delta.x;
            _body.rotation = Quaternion.Euler(0f, _yaw, 0f);

            _pitch = Mathf.Clamp(_pitch - delta.y, -85f, 85f);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // ---- Move -------------------------------------------------------

        void HandleMove(float deltaTime)
        {
            var   kb = Keyboard.current;
            float h  = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v  = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            Vector3 horizontal = (_body.right * h + _body.forward * v).normalized * _moveSpeed;
            _rb.linearVelocity = new Vector3(horizontal.x, _rb.linearVelocity.y, horizontal.z);
        }

        // ---- Shoot ------------------------------------------------------

        void HandleShoot()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            // Origen: boca del cañón. Dirección: hacia donde mira la cámara.
            _projectileSystem.Fire(_shootPoint.position, _cameraPivot.forward);
        }
    }
}
