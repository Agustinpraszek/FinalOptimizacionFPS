using TMPro;
using UnityEngine;

public sealed class GameUIBridge : MonoBehaviour
{
    // Recibe eventos de gameplay y actualiza los textos de la UI

    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _outcomeText;

    private int _totalWaves;

    public void Init(int totalWaves)
    {
        _totalWaves = totalWaves;
        if (_outcomeText != null) _outcomeText.gameObject.SetActive(false);
    }

    public void SetWave(int current)
    {
        _waveText.text = $"WAVE {current}/{_totalWaves}";
    }

    public void SetHealth(int hp)
    {
        _healthText.text = $"HP  {hp}";
    }
    public void SetEnemyCount(int count)
    {
        _enemyCountText.text = $"ENEMIES  {count}";
    }

    public void ShowVictory()
    {
        ShowOutcome("YOU WON");
    }

    public void ShowDefeat()
    {
        ShowOutcome("YOU LOSE");
    }

    private void ShowOutcome(string message)
    {
        if (_outcomeText == null) return;
        _outcomeText.text = message;
        _outcomeText.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
