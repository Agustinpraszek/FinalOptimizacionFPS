using System;
using UnityEngine;
using UnityEngine.InputSystem;
public sealed class PlayerLogic : ITickable
{
    // Lógica del jugador: movimiento, cámara y disparo

    private readonly Transform _body;
    private readonly Transform _cameraPivot;
    private readonly Transform _shootPoint;
    private readonly Rigidbody _rb;
    private readonly WaveManager _waveManager;
    private readonly ProjectileSystem _projectileSystem;
    private readonly Animator _pistolAnimator;

    private static readonly int FireHash = Animator.StringToHash("Fire");

    private readonly float _moveSpeed;
    private readonly float _mouseSensitivity;

    private float _pitch;
    private float _yaw;
    private int _health;

    public int  Health => _health;
    public bool IsDead => _health <= 0;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;

    public PlayerLogic(Transform body, Transform cameraPivot, Transform shootPoint, Rigidbody rb, WaveManager waveManager, ProjectileSystem projectileSystem, PlayerSettings settings, Animator pistolAnimator)
    {
        _body = body;
        _cameraPivot = cameraPivot;
        _shootPoint = shootPoint;
        _rb = rb;
        _waveManager = waveManager;
        _projectileSystem = projectileSystem;
        _pistolAnimator = pistolAnimator;
        _moveSpeed = settings.MoveSpeed;
        _mouseSensitivity = settings.MouseSensitivity;
        _health = settings.MaxHealth;

        _rb.freezeRotation = true;
        _yaw = _body.eulerAngles.y;
    }

    public void Tick(float deltaTime)
    {
        if (IsDead) return;
        HandleLook();
        HandleMove();
        HandleShoot();
    }

    public void TakeDamage(int amount)
    {
        _health = Mathf.Max(0, _health - amount);
        OnHealthChanged?.Invoke(_health);
        if (IsDead) OnDeath?.Invoke();
    }

    private void HandleLook()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * (_mouseSensitivity * 0.1f);

        _yaw += delta.x;
        _body.rotation = Quaternion.Euler(0f, _yaw, 0f);

        _pitch = Mathf.Clamp(_pitch - delta.y, -85f, 85f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        var   kb = Keyboard.current;
        float h  = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v  = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 dir = (_body.right * h + _body.forward * v).normalized * _moveSpeed;
        _rb.linearVelocity = new Vector3(dir.x, _rb.linearVelocity.y, dir.z);
    }

    private void HandleShoot()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        _projectileSystem.Fire(_shootPoint.position, _cameraPivot.forward);
        _pistolAnimator.SetTrigger(FireHash);
    }
}
