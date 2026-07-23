using System;
using UnityEngine;
using Object = UnityEngine.Object;

// Los tres efectos del juego. Todos opcionales. Si falta alguno el juego corre
// igual, solo se pierde ese efecto.
[Serializable]
public sealed class VfxSettings
{
    [Tooltip("Al impactar un proyectil.")]
    public VfxData Impact;

    [Tooltip("Al morir un enemigo.")]
    public VfxData EnemyDeath;

    [Tooltip("Al disparar.")]
    public VfxData MuzzleFlash;

    // Se llama una sola vez al arrancar. Avisa qué falta y dónde asignarlo, en
    // vez de loguear en cada spawn y llenar la consola.
    public void LogMissing(Object context)
    {
        WarnIfMissing(Impact, "Impact", context);
        WarnIfMissing(EnemyDeath, "Enemy Death", context);
        WarnIfMissing(MuzzleFlash, "Muzzle Flash", context);
    }

    private static void WarnIfMissing(VfxData data, string fieldName, Object context)
    {
        if (data == null)
        {
            Debug.LogWarning(
                $"[VFX] Falta el asset en GameConfig > Vfx > {fieldName}. " +
                "El juego corre sin ese efecto.", context);
            return;
        }

        if (!data.IsUsable)
        {
            Debug.LogWarning(
                $"[VFX] El asset '{data.name}' (GameConfig > Vfx > {fieldName}) " +
                "no tiene Prefab asignado. El juego corre sin ese efecto.", context);
        }
    }
}
