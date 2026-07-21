using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Muestra el modelo en primera persona del arma equipada.
// Instancia todas las vistas una sola vez al arrancar y después solo prende y
// apaga: no hay Instantiate ni Destroy durante el gameplay.
// Si a un arma le falta el prefab, avisa una vez y se juega sin modelo.
public sealed class WeaponViewBinder
{
    private readonly Dictionary<WeaponData, GameObject> _views;

    private GameObject _current;

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

            // Instantiate con parent conserva la posición local del prefab, así
            // cada arma trae su propio encuadre y el pivot es solo el ancla.
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
    }
}
