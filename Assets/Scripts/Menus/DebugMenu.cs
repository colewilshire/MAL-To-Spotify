using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : Menu
{
    protected override MenuState ActiveState {get; set;} = MenuState.Debug;
    [SerializeField] private Button printSavedMALTokenButton;
    [SerializeField] private Button printSavedSpotifyTokenButton;
    [SerializeField] private Button deleteSavedMALTokenButton;
    [SerializeField] private Button deleteSavedSpotifyTokenButton;
    [SerializeField] private Button printSavedSongListButton;
    [SerializeField] private Button exportSavedSongListButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TMP_InputField console;

    protected override void Start()
    {
        base.Start();

        printSavedMALTokenButton.onClick.AddListener(() => PrintSavedToken(AuthenticationController.Instance.MalTokenSaveName));
        printSavedSpotifyTokenButton.onClick.AddListener(() => PrintSavedToken(AuthenticationController.Instance.SpotifyTokenSaveName));
        deleteSavedMALTokenButton.onClick.AddListener(() => DeleteSavedToken(AuthenticationController.Instance.MalTokenSaveName));
        deleteSavedSpotifyTokenButton.onClick.AddListener(() => DeleteSavedToken(AuthenticationController.Instance.SpotifyTokenSaveName));
        printSavedSongListButton.onClick.AddListener(PrintSavedSongList);
        exportSavedSongListButton.onClick.AddListener(() => DebugController.Instance.ExportSongList());
        mainMenuButton.onClick.AddListener(() => MenuController.Instance.SetMenu(MenuState.Main));
    }

    private void PrintSavedToken(string tokenSaveName)
    {
        string serializedToken = AuthenticationController.Instance.GetTokenSaveData(tokenSaveName);
        console.text = serializedToken;
    }

    private void DeleteSavedToken(string tokenSaveName)
    {
        AuthenticationController.Instance.DeleteSavedToken(tokenSaveName);
    }

    private void PrintSavedSongList()
    {
        string serializedSongList = MALController.Instance.GetSerializedSongList();
        console.text = serializedSongList;
    }
}
