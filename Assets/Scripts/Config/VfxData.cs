using UnityEngine;

// Un efecto visual pooleado. El prefab debe tener un ParticleSystem (propio o en
// un hijo). Duration es cuánto tarda en volver al pool, así que conviene que sea
// igual o un poco mayor que la duración real del sistema de partículas.
[CreateAssetMenu(menuName = "Game/Vfx Data", fileName = "VfxData")]
public sealed class VfxData : ScriptableObject
{
    public GameObject Prefab;

    [Min(0.05f)] public float Duration = 1f;
    [Min(1)] public int PrewarmCount = 8;

    public bool IsUsable => Prefab != null;
}
