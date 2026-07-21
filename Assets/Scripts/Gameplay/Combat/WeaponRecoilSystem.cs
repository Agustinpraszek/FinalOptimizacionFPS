using UnityEngine;

// Retroceso procedural del arma, en dos etapas:
//   _target  es hacia dónde empuja el retroceso acumulado, y decae solo a cero.
//   _current persigue a _target más rápido, y es lo que se ve.
//
// Esa separación es la que aguanta cadencias altas: los disparos del rifle se
// suman a _target, que ya está volviendo a cero, así que el retroceso se
// estabiliza en un valor fijo en vez de acumularse sin freno.
//
// Solo rota el pivot del arma, nunca la cámara. Si tocara la cámara pelearía
// contra el pitch que PlayerLogic escribe todos los frames.
public sealed class WeaponRecoilSystem : IUpdatable
{
    private const float RestThreshold = 0.0001f;

    private readonly Transform _pivot;

    private Vector3 _target;
    private Vector3 _current;

    // Se guardan del último disparo, así el arma termina de recuperarse con su
    // propia sensación aunque cambies de arma a mitad del retroceso.
    private float _recoverySpeed = 9f;
    private float _snappiness = 22f;

    private bool _atRest = true;

    public WeaponRecoilSystem(Transform weaponPivot)
    {
        _pivot = weaponPivot;
    }

    public void AddKick(WeaponRecoil recoil)
    {
        if (_pivot == null || recoil == null) return;

        _recoverySpeed = recoil.RecoverySpeed;
        _snappiness = recoil.Snappiness;

        float side = Random.Range(-recoil.KickSide, recoil.KickSide);

        // Pitch negativo levanta el caño. El roll sale del lateral, así el giro
        // acompaña el desvío sin necesidad de otro parámetro.
        _target += new Vector3(-recoil.KickUp, side, -side * 0.5f);
        _target = Vector3.ClampMagnitude(_target, recoil.MaxOffset);

        _atRest = false;
    }

    public void Reset()
    {
        _target = Vector3.zero;
        _current = Vector3.zero;

        if (_pivot != null) _pivot.localRotation = Quaternion.identity;
        _atRest = true;
    }

    public void Tick(float deltaTime)
    {
        // En reposo no toca el transform: sin disparos, este sistema no cuesta nada.
        if (_pivot == null || _atRest) return;

        // Decaimiento exponencial en vez de Lerp con deltaTime crudo: así el
        // retroceso se siente igual a 30 que a 144 FPS.
        _target = Vector3.Lerp(_target, Vector3.zero, Decay(_recoverySpeed, deltaTime));
        _current = Vector3.Lerp(_current, _target, Decay(_snappiness, deltaTime));

        if (_current.sqrMagnitude < RestThreshold && _target.sqrMagnitude < RestThreshold)
        {
            Reset();
            return;
        }

        _pivot.localRotation = Quaternion.Euler(_current);
    }

    private static float Decay(float speed, float deltaTime)
    {
        return 1f - Mathf.Exp(-speed * deltaTime);
    }
}
