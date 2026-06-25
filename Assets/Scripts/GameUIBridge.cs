using TMPro;
using UnityEngine;

public sealed class GameUIBridge : MonoBehaviour
{
    // Recibe eventos de gameplay y actualiza los textos de la UI

    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _enemyCountText;

    private int _totalWaves;

    public void Init(int totalWaves) => _totalWaves = totalWaves;

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
}
