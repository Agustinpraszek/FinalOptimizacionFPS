using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct MainMenuReferences
{
    [SerializeField] private Button _btnStart;
    [SerializeField] private Button _btnQuit;

    public Button BtnStart => _btnStart;
    public Button BtnQuit => _btnQuit;
}

[Serializable]
public struct PauseMenuReferences
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _btnResume;
    [SerializeField] private Button _btnMainMenu;

    public GameObject Panel => _panel;
    public Button BtnResume => _btnResume;
    public Button BtnMainMenu => _btnMainMenu;
}

// Único puente entre la escena y el código puro. Expone lo que hay que cablear
// en el Inspector, sin lógica.
public sealed class SceneReferences : MonoBehaviour
{
    [SerializeField] private PlayerReferences _player;
    [SerializeField] private HudReferences _hud;
    [SerializeField] private MainMenuReferences _mainMenu;
    [SerializeField] private PauseMenuReferences _pauseMenu;
    [SerializeField] private Transform[] _enemySpawnPoints;
    [SerializeField] private BuyStationSetup[] _buyStations;

    public PlayerReferences Player => _player;
    public HudReferences Hud => _hud;
    public MainMenuReferences MainMenu => _mainMenu;
    public PauseMenuReferences PauseMenu => _pauseMenu;
    public Transform[] EnemySpawnPoints => _enemySpawnPoints;
    public BuyStationSetup[] BuyStations => _buyStations;
}