using TMPro;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// MonoBehaviour #4. Puente entre los eventos de gameplay y la UI.
    /// No tiene lógica: solo actualiza texto cuando los POCOs disparan eventos.
    ///
    /// Setup en Unity:
    ///   1. Agregar este script a un GameObject vacío en la escena (ej. "UIManager").
    ///   2. Crear un Canvas con tres TextMeshPro y arrastrarlos aquí.
    ///   3. Arrastrar este GameObject al campo _uiBridge del GameBootstrap.
    /// </summary>
    public sealed class GameUIBridge : MonoBehaviour
    {
        [SerializeField] TMP_Text _waveText;
        [SerializeField] TMP_Text _healthText;
        [SerializeField] TMP_Text _enemyCountText;

        int _totalWaves;

        /// <summary>Llamado por Bootstrap para que la UI sepa el total de waves.</summary>
        public void Init(int totalWaves) => _totalWaves = totalWaves;

        public void SetWave(int current)
            => _waveText.text = $"WAVE {current}/{_totalWaves}";

        public void SetHealth(int hp)
            => _healthText.text = $"HP  {hp}";

        public void SetEnemyCount(int count)
            => _enemyCountText.text = $"ENEMIES  {count}";
    }
}
