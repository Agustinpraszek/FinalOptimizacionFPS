using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Arma equipada, cadencia, dispersión y qué armas tiene el jugador.
// Le pide disparos al ProjectileSystem; no sabe nada de pools ni de enemigos.
public sealed class WeaponSystem : IUpdatable
{
    private static readonly Key[] SlotKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5
    };

    private readonly Transform _shootPoint;
    private readonly Transform _aimPivot;
    private readonly ProjectileSystem _projectileSystem;
    private readonly IReadOnlyList<WeaponData> _weapons;
    private readonly bool[] _owned;
    private readonly LayerMask _hitMask;

    private int _currentIndex;
    private float _cooldown;

    public WeaponData CurrentWeapon => _weapons[_currentIndex];

    public event Action<WeaponData> OnWeaponChanged;

    // Origen y dirección del disparo. Lo va a consumir el muzzle flash.
    public event Action<Vector3, Vector3> OnShotFired;

    public WeaponSystem(
        Transform shootPoint,
        Transform aimPivot,
        ProjectileSystem projectileSystem,
        IReadOnlyList<WeaponData> weapons,
        LayerMask hitMask)
    {
        _shootPoint = shootPoint;
        _aimPivot = aimPivot;
        _projectileSystem = projectileSystem;
        _weapons = weapons;
        _hitMask = hitMask;

        // La primera arma de la lista viene desbloqueada.
        _owned = new bool[weapons.Count];
        _owned[0] = true;
        _currentIndex = 0;
    }

    public bool IsOwned(WeaponData weapon)
    {
        int index = IndexOf(weapon);
        return index >= 0 && _owned[index];
    }

    // La usa la tienda una vez que cobró. Equipa el arma recién comprada.
    public bool Unlock(WeaponData weapon)
    {
        int index = IndexOf(weapon);
        if (index < 0 || _owned[index]) return false;

        _owned[index] = true;
        Equip(index);
        return true;
    }

    public void Tick(float deltaTime)
    {
        if (_cooldown > 0f) _cooldown -= deltaTime;

        HandleSlotInput();
        HandleFireInput();
    }

    private void HandleSlotInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        int slots = Mathf.Min(_weapons.Count, SlotKeys.Length);
        for (int i = 0; i < slots; i++)
        {
            if (keyboard[SlotKeys[i]].wasPressedThisFrame) Equip(i);
        }
    }

    private void HandleFireInput()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || _cooldown > 0f) return;

        WeaponData weapon = CurrentWeapon;
        bool wantsToFire = weapon.IsAutomatic
            ? mouse.leftButton.isPressed
            : mouse.leftButton.wasPressedThisFrame;

        if (!wantsToFire) return;

        Fire(weapon);
        _cooldown = 1f / weapon.FireRate;
    }

    private void Fire(WeaponData weapon)
    {
        Vector3 origin = _shootPoint.position;
        Vector3 forward = _aimPivot.forward;

        var config = new ProjectileConfig(
            weapon.ProjectileSpeed,
            weapon.ProjectileLifetime,
            weapon.Damage,
            _hitMask);

        for (int i = 0; i < weapon.ProjectilesPerShot; i++)
        {
            Vector3 direction = ApplySpread(forward, weapon.SpreadAngle);
            _projectileSystem.Fire(origin, direction, in config);
        }

        OnShotFired?.Invoke(origin, forward);
    }

    // El cono se arma sobre el eje del disparo y no sobre ejes de mundo, para que
    // la dispersión sea igual mirando al horizonte que mirando al piso.
    private static Vector3 ApplySpread(Vector3 forward, float spreadAngle)
    {
        if (spreadAngle <= 0f) return forward;

        float half = spreadAngle * 0.5f;
        Quaternion offset = Quaternion.Euler(
            UnityEngine.Random.Range(-half, half),
            UnityEngine.Random.Range(-half, half),
            0f);

        return Quaternion.LookRotation(forward) * offset * Vector3.forward;
    }

    private void Equip(int index)
    {
        if (index == _currentIndex || !_owned[index]) return;

        _currentIndex = index;
        _cooldown = 0f;
        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    private int IndexOf(WeaponData weapon)
    {
        for (int i = 0; i < _weapons.Count; i++)
        {
            if (_weapons[i] == weapon) return i;
        }

        return -1;
    }
}
