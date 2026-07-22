using UnityEngine;

// Único puente entre la escena y el código puro. Expone lo que hay que cablear
// en el Inspector, sin lógica.
public sealed class SceneReferences : MonoBehaviour
{
    [SerializeField] private PlayerReferences _player;
    [SerializeField] private HudReferences _hud;
    [SerializeField] private Transform[] _enemySpawnPoints;
    [SerializeField] private BuyStationSetup[] _buyStations;

    public PlayerReferences Player => _player;
    public HudReferences Hud => _hud;
    public Transform[] EnemySpawnPoints => _enemySpawnPoints;
    public BuyStationSetup[] BuyStations => _buyStations;
}
