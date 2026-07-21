using System;
using TMPro;
using UnityEngine;

// Referencias de escena del HUD. Solo datos: la lógica está en HudPresenter.
[Serializable]
public sealed class HudReferences
{
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _enemyCountText;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _weaponText;
    [SerializeField] private TMP_Text _shopPromptText;
    [SerializeField] private TMP_Text _outcomeText;

    public TMP_Text WaveText => _waveText;
    public TMP_Text HealthText => _healthText;
    public TMP_Text EnemyCountText => _enemyCountText;
    public TMP_Text MoneyText => _moneyText;
    public TMP_Text WeaponText => _weaponText;
    public TMP_Text ShopPromptText => _shopPromptText;
    public TMP_Text OutcomeText => _outcomeText;
}
