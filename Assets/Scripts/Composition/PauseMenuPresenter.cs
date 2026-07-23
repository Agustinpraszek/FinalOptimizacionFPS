using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PauseMenuPresenter : IUpdatable
{
    private readonly PauseMenuReferences _references;
    private readonly UpdateManager _updateManager;
    private readonly string _mainMenuSceneName;

    private bool _isPaused;

    public PauseMenuPresenter(PauseMenuReferences references, UpdateManager updateManager, string mainMenuSceneName = "MainMenu")
    {
        _references = references;
        _updateManager = updateManager;
        _mainMenuSceneName = mainMenuSceneName;

        if (_references.Panel != null) _references.Panel.SetActive(false);

        if (_references.BtnResume != null) _references.BtnResume.onClick.AddListener(ResumeGame);
        if (_references.BtnMainMenu != null) _references.BtnMainMenu.onClick.AddListener(GoToMainMenu);
    }

    public void Tick(float deltaTime)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (_isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;
        _updateManager.SetPaused(true);

        // Pausa todos los animators de la escena
        foreach (Animator animator in Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
        {
            animator.speed = 0f;
        }

        if (_references.Panel != null) _references.Panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        _updateManager.SetPaused(false);

        // Reanuda los animators
        foreach (Animator animator in Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
        {
            animator.speed = 1f;
        }

        if (_references.Panel != null) _references.Panel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void GoToMainMenu()
    {
        _updateManager.SetPaused(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene(_mainMenuSceneName);
    }

    public void Dispose()
    {
        if (_references.BtnResume != null) _references.BtnResume.onClick.RemoveListener(ResumeGame);
        if (_references.BtnMainMenu != null) _references.BtnMainMenu.onClick.RemoveListener(GoToMainMenu);
    }
}