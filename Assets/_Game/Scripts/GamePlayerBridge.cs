using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #3 (último de 3 permitidos). Puente fino entre Unity y el POCO PlayerLogic.
    /// Solo hace dos cosas en Awake(): bloquear el cursor y exponer referencias de componentes.
    /// Cero lógica de gameplay acá.
    /// </summary>
    public sealed class GamePlayerBridge : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        /// <summary>Asignar en Inspector: la Main Camera como hijo del Player.</summary>
        public Camera PlayerCamera => _camera;
        public CharacterController CharacterController { get; private set; }

        void Awake()
        {
            CharacterController = GetComponent<CharacterController>();

            if (_camera == null)
                _camera = GetComponentInChildren<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
