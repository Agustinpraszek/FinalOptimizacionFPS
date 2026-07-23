using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class WeaponViewBinder
{
    private static readonly int FireTrigger = Animator.StringToHash("Fire");

    private readonly Dictionary<WeaponData, GameObject> _views;

    private GameObject _current;

    private Animator _currentAnimator;

    public WeaponViewBinder(Transform pivot, IReadOnlyList<WeaponData> weapons)
    {
        _views = new Dictionary<WeaponData, GameObject>(weapons.Count);

        if (pivot == null)
        {
            Debug.LogWarning(
                "[Weapon] SceneReferences > Player > Weapon Pivot sin asignar. " +
                "No se van a ver los modelos de arma.");
            return;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponData weapon = weapons[i];
            if (weapon == null) continue;

            if (weapon.ViewPrefab == null)
            {
                Debug.LogWarning(
                    $"[Weapon] '{weapon.name}' no tiene View Prefab asignado. " +
                    "Esa arma se va a usar sin modelo.", weapon);
                continue;
            }

            GameObject view = Object.Instantiate(weapon.ViewPrefab, pivot);
            view.SetActive(false);
            _views[weapon] = view;
        }
    }

    public void Show(WeaponData weapon)
    {
        GameObject next = null;
        if (weapon != null) _views.TryGetValue(weapon, out next);

        if (next == _current) return;

        if (_current != null) _current.SetActive(false);
        if (next != null) next.SetActive(true);

        _current = next;
        _currentAnimator = next != null ? next.GetComponent<Animator>() : null;
    }

    public void PlayFire()
    {
        if (_currentAnimator == null) return;
        _currentAnimator.SetTrigger(FireTrigger);
    }
}
