using UnityEngine;

// Un arma. Sumar una es crear otro asset y cargarlo en GameConfig.Weapons.
// El radio del proyectil y la máscara de impacto son globales y viven en GameConfig.
// El precio no está acá, vive en el WeaponPurchase que la vende.
[CreateAssetMenu(menuName = "Game/Weapon Data", fileName = "WeaponData")]
public sealed class WeaponData : ScriptableObject
{
    public string DisplayName = "Pistol";

    [Header("Disparo")]
    [Min(1)] public int Damage = 25;

    [Tooltip("Disparos por segundo.")]
    [Min(0.1f)] public float FireRate = 4f;

    [Tooltip("Proyectiles por disparo. La escopeta usa varios.")]
    [Min(1)] public int ProjectilesPerShot = 1;

    [Tooltip("Apertura del cono en grados. 0 es puntería perfecta.")]
    [Min(0f)] public float SpreadAngle = 0f;

    [Tooltip("Si es automática dispara manteniendo el click.")]
    public bool IsAutomatic = false;

    [Header("Proyectil")]
    [Min(1f)] public float ProjectileSpeed = 30f;
    [Min(0.1f)] public float ProjectileLifetime = 4f;

    [Header("Retroceso")]
    public WeaponRecoil Recoil;

    [Header("Vista")]
    [Tooltip("Modelo en primera persona. Conserva su posición local, así que el " +
             "encuadre de cada arma se ajusta desde su propio prefab.")]
    public GameObject ViewPrefab;
}
