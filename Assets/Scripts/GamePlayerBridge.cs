using UnityEngine;

public sealed class GamePlayerBridge : MonoBehaviour
{
    //Conecta todos los objetos para que el bootstrap pueda funcionar con clases puras y objetos de unity

    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _shootPoint;

    public Transform CameraPivot  => _cameraPivot;
    public Camera PlayerCamera => _camera;
    public Transform ShootPoint => _shootPoint;
    public Rigidbody Rigidbody { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
