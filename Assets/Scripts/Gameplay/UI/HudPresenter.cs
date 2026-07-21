// Pasa eventos de gameplay a texto en pantalla. No conoce WaveManager ni
// PlayerLogic, recibe los valores ya resueltos.
// Los métodos allocan string, pero corren por evento y no por frame.
public sealed class HudPresenter
{
    private readonly HudReferences _refs;
    private readonly int _totalWaves;

    public HudPresenter(HudReferences refs, int totalWaves)
    {
        _refs = refs;
        _totalWaves = totalWaves;

        if (_refs.OutcomeText != null)
            _refs.OutcomeText.gameObject.SetActive(false);
    }

    public void SetWave(int current)
    {
        if (_refs.WaveText != null) _refs.WaveText.text = $"WAVE {current}/{_totalWaves}";
    }

    public void SetHealth(int health)
    {
        if (_refs.HealthText != null) _refs.HealthText.text = $"HP  {health}";
    }

    public void SetEnemyCount(int count)
    {
        if (_refs.EnemyCountText != null) _refs.EnemyCountText.text = $"ENEMIES  {count}";
    }

    public void ShowVictory() => ShowOutcome("YOU WON");

    public void ShowDefeat() => ShowOutcome("YOU LOSE");

    private void ShowOutcome(string message)
    {
        if (_refs.OutcomeText == null) return;

        _refs.OutcomeText.text = message;
        _refs.OutcomeText.gameObject.SetActive(true);
    }
}
