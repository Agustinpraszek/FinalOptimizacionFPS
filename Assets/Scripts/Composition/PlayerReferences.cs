using System;
using UnityEngine;

// Referencias de escena del jugador. Serializable y no MonoBehaviour, así agrupa
// datos dentro de SceneReferences sin gastar uno de los tres scripts permitidos.
[Serializable]
public sealed class PlayerReferences
{
    [SerializeField] private Transform _body;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Rigidbody _rigidbody;

    [Tooltip("Ancla de los modelos de arma. Va como hijo de CameraPivot en (0,0,0).")]
    [SerializeField] private Transform _weaponPivot;

    public Transform Body => _body;
    public Transform CameraPivot => _cameraPivot;
    public Transform ShootPoint => _shootPoint;
    public Rigidbody Rigidbody => _rigidbody;
    public Transform WeaponPivot => _weaponPivot;

    public bool IsValid(out string error)
    {
        if (_body == null) { error = "PlayerReferences.Body sin asignar."; return false; }
        if (_cameraPivot == null) { error = "PlayerReferences.CameraPivot sin asignar."; return false; }
        if (_shootPoint == null) { error = "PlayerReferences.ShootPoint sin asignar."; return false; }
        if (_rigidbody == null) { error = "PlayerReferences.Rigidbody sin asignar."; return false; }

        error = null;
        return true;
    }
}
