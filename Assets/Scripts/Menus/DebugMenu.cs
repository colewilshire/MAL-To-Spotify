using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : Menu
{
    protected override MenuState ActiveState {get; set;} = MenuState.Debug;
    [SerializeField] private Button outputSavedMALTokenButton;
    [SerializeField] private Button outputSavedSpotifyTokenButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TMP_InputField console;

    protected override void Start()
    {
        base.Start();

        outputSavedMALTokenButton.onClick.AddListener(OutputSavedMALToken);
        outputSavedSpotifyTokenButton.onClick.AddListener(OutputSavedSpotifyToken);
        mainMenuButton.onClick.AddListener(() => MenuController.Instance.SetMenu(MenuState.Main));
    }

    private void OutputSavedMALToken()
    {
        string serializedToken = AuthenticationController.Instance.GetTokenSaveData(AuthenticationController.Instance.MalTokenSaveName);
        console.text = serializedToken;
    }

    private void OutputSavedSpotifyToken()
    {
        string serializedToken = AuthenticationController.Instance.GetTokenSaveData(AuthenticationController.Instance.SpotifyTokenSaveName);
        console.text = serializedToken;
    }
}
