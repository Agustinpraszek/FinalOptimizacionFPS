using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Estado de la partida: fin de juego, cursor, reinicio y salida.
// Va en el canal Always para seguir leyendo input con el gameplay pausado.
public sealed class GameFlowSystem : IUpdatable
{
    private readonly UpdateManager _updateManager;
    private bool _isGameOver;

    public bool IsGameOver => _isGameOver;

    public GameFlowSystem(UpdateManager updateManager)
    {
        _updateManager = updateManager;
        SetCursorLocked(true);
    }

    // Entrada de victoria y derrota. Congela el gameplay sin frenar este sistema.
    public void EndGame()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        _updateManager.SetPaused(true);

        // Frena física, animators y partículas. El SetPaused ya cortó los canales
        // de gameplay del loop.
        Time.timeScale = 0f;

        SetCursorLocked(false);
    }

    public void Tick(float deltaTime)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame) Quit();

        if (_isGameOver && keyboard.rKey.wasPressedThisFrame) Restart();
    }

    private static void Restart()
    {
        // Si no lo reseteo, la escena nueva arranca congelada por el timeScale del game over.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
