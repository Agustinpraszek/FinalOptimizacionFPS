using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Movimiento, cámara y vida del jugador. El disparo lo maneja WeaponSystem.
// Implementa IDamageable, así cualquier fuente de daño lo afecta sin conocerlo.
public sealed class PlayerLogic : IUpdatable, IDamageable
{
    private readonly Transform _body;
    private readonly Transform _cameraPivot;
    private readonly Rigidbody _rigidbody;

    private readonly float _moveSpeed;
    private readonly float _mouseSensitivity;
    private readonly int _maxHealth;

    private float _pitch;
    private float _yaw;
    private int _health;

    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public bool IsAlive => _health > 0;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    public PlayerLogic(Transform body, Transform cameraPivot, Rigidbody rigidbody, PlayerSettings settings)
    {
        _body = body;
        _cameraPivot = cameraPivot;
        _rigidbody = rigidbody;

        _moveSpeed = settings.MoveSpeed;
        _mouseSensitivity = settings.MouseSensitivity;
        _maxHealth = settings.MaxHealth;
        _health = settings.MaxHealth;

        _rigidbody.freezeRotation = true;
        _yaw = _body.eulerAngles.y;
    }

    public void Tick(float deltaTime)
    {
        if (!IsAlive) return;

        HandleLook();
        HandleMove();
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        _health = Mathf.Max(0, _health - amount);
        OnHealthChanged?.Invoke(_health);

        if (!IsAlive) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        int healed = Mathf.Min(_maxHealth, _health + amount);
        if (healed == _health) return;

        _health = healed;
        OnHealthChanged?.Invoke(_health);
    }

    private void HandleLook()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue() * (_mouseSensitivity * 0.1f);

        _yaw += delta.x;
        _rigidbody.MoveRotation(Quaternion.Euler(0f, _yaw, 0f));

        _pitch = Mathf.Clamp(_pitch - delta.y, -85f, 85f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float horizontal = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float vertical = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);

        Vector3 direction = (_body.right * horizontal + _body.forward * vertical).normalized * _moveSpeed;
        Vector3 velocity = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(direction.x, velocity.y, direction.z);
    }

}
