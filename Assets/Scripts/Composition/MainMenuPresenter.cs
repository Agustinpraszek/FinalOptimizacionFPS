using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class MainMenuPresenter
{
    private readonly Button _btnStart;
    private readonly Button _btnQuit;
    private readonly string _targetSceneName;

    public MainMenuPresenter(Button btnStart, Button btnQuit, string targetSceneName = "Scene 2")
    {
        _btnStart = btnStart;
        _btnQuit = btnQuit;
        _targetSceneName = targetSceneName;

        // Por si se llegó acá con el juego pausado (timeScale en 0): el menú
        // nunca debe arrancar congelado, sin importar desde dónde se cargó.
        Time.timeScale = 1f;

        // Forzar visibilidad y desbloqueo del cursor para el menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_btnStart != null) _btnStart.onClick.AddListener(OnStartClicked);
        if (_btnQuit != null) _btnQuit.onClick.AddListener(OnQuitClicked);
    }

    public void Dispose()
    {
        if (_btnStart != null) _btnStart.onClick.RemoveListener(OnStartClicked);
        if (_btnQuit != null) _btnQuit.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene(_targetSceneName);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}