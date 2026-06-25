using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #3. Puente entre Unity y los POCOs de gameplay.
    /// Solo expone referencias — cero lógica acá.
    ///
    /// Jerarquía esperada:
    ///   Player  (Rigidbody + CapsuleCollider + este script)
    ///   └── CameraPivot  (empty — recibe pitch del script)
    ///       └── Main Camera
    ///       └── Arma
    ///           └── ShootPoint  (empty en la boca del cañón)
    /// </summary>
    public sealed class GamePlayerBridge : MonoBehaviour
    {
        [SerializeField] Transform _cameraPivot;
        [SerializeField] Camera    _camera;
        [SerializeField] Transform _shootPoint;

        public Transform CameraPivot  => _cameraPivot;
        public Camera    PlayerCamera => _camera;
        public Transform ShootPoint   => _shootPoint;
        public Rigidbody Rigidbody    { get; private set; }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();

            if (_camera == null)
                _camera = GetComponentInChildren<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }
}
