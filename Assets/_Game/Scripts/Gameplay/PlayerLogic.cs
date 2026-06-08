using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// POCO (sin MonoBehaviour). Toda la lógica de primera persona:
    /// movimiento, mouse-look y disparo por raycast.
    /// Se tickea desde el UpdateManager, jamás usa Update() nativo.
    /// </summary>
    public sealed class PlayerLogic : ITickable
    {
        readonly Transform _body;
        readonly Transform _cameraTransform;
        readonly CharacterController _controller;
        readonly WaveManager _waveManager;

        readonly float _moveSpeed;
        readonly float _mouseSensitivity;
        readonly float _gravity;
        readonly float _shootRange;

        float _pitch;
        float _verticalVelocity;
        int _health;

        public int Health => _health;
        public bool IsDead => _health <= 0;

        public PlayerLogic(Transform body, Transform cameraTransform,
            CharacterController controller, WaveManager waveManager,
            PlayerSettings settings)
        {
            _body             = body;
            _cameraTransform  = cameraTransform;
            _controller       = controller;
            _waveManager      = waveManager;
            _moveSpeed        = settings.MoveSpeed;
            _mouseSensitivity = settings.MouseSensitivity;
            _gravity          = settings.Gravity;
            _shootRange       = settings.ShootRange;
            _health           = settings.MaxHealth;
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
            if (IsDead)
                Debug.Log("[Player] ¡Jugador muerto!");
        }

        // ---- Look -------------------------------------------------------

        void HandleLook()
        {
            Vector2 delta = Mouse.current.delta.ReadValue() * _mouseSensitivity * 0.1f;

            // Rotación horizontal: rota el cuerpo completo.
            _body.Rotate(Vector3.up, delta.x);

            // Rotación vertical: solo la cámara, limitada para no "voltear".
            _pitch = Mathf.Clamp(_pitch - delta.y, -85f, 85f);
            _cameraTransform.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }

        // ---- Move -------------------------------------------------------

        void HandleMove(float deltaTime)
        {
            var kb = Keyboard.current;
            float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

            Vector3 move = (_body.right * h + _body.forward * v).normalized * _moveSpeed;

            // Gravedad simple: resetea si está en el suelo.
            if (_controller.isGrounded)
                _verticalVelocity = -1f;   // pequeña fuerza hacia abajo para mantenerse grounded
            else
                _verticalVelocity -= _gravity * deltaTime;

            move.y = _verticalVelocity;
            _controller.Move(move * deltaTime);
        }

        // ---- Shoot ------------------------------------------------------

        void HandleShoot()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            // Raycast desde el centro de la cámara hacia adelante.
            // Debug.DrawRay es visible en la Scene view con Gizmos activados.
            Debug.DrawRay(_cameraTransform.position, _cameraTransform.forward * _shootRange,
                Color.red, 0.1f);

            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward,
                    out RaycastHit hit, _shootRange))
            {
                _waveManager.TryKill(hit.collider.gameObject);
            }
        }
    }
}
