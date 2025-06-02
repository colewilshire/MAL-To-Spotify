using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MALController : Singleton<MALController>
{
    [SerializeField] private TMP_InputField malInputField;
    [SerializeField] private Button malLoginButton;
    [SerializeField] private Button spotifyLoginButton;
    private MALClient malClient;
    private AnimeListResponse fullAnimeList;
    private readonly int requestDelay = 1100;
    public Dictionary<int, Theme> OpeningThemes;
    public string SongListSaveName = "OpeningThemes";
    public string UpdatedAtSaveName = "UpdatedAt";
    public string SaveFileExtension = "txt";

    private void Start()
    {
        malLoginButton.onClick.AddListener(async () => await OnMalLogin());
    }

    private async Task OnMalLogin()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);

        malClient = await AuthenticationController.Instance.AuthenticateMALClient();
        OpeningThemes = LoadThemeSongList();
        malInputField.text = (await malClient.GetMyUserInfoAsync()).Name;

        while (fullAnimeList == null)
        {
            fullAnimeList = await GetAnimeListAsync();
        }

        List<int> updatedAnime = CheckForUpdates(fullAnimeList);
        if (updatedAnime.Count > 0)
        {
            List<Theme> allThemes = new();
            List<Theme> tempOpeningThemes = await GetAnimeListThemeSongsAsync(updatedAnime);

            allThemes.AddRange(tempOpeningThemes);
            OpeningThemes.AddRange(allThemes
                .GroupBy(theme => theme.Id)
                .ToDictionary(group => group.Key, group => group.First()));

            SaveThemeSongList(OpeningThemes);
        }

        malLoginButton.interactable = false;
        spotifyLoginButton.interactable = true;

        MenuController.Instance.SetMenu(MenuState.Main);
    }

    public async Task<AnimeListResponse> GetAnimeListAsync(string nextPageUrl = null)
    {
        AnimeListResponse animeList;

        MenuController.Instance.UpdateProgressBar(0, $"Loading...");

        if (nextPageUrl != null)
        {
            animeList = await malClient.GetAnimeListNextPageAsync(nextPageUrl);
        }
        else
        {
            animeList = await malClient.GetAnimeListAsync("@me", new(){ AnimeField.UpdatedAt });
        }

        if (animeList == null)
        {
            MenuController.Instance.UpdateProgressBar(0, $"Returning...");
            return null;
        }

        if (animeList.Paging.Next != null)
        {
            AnimeListResponse nextAnimeList = await GetAnimeListAsync(animeList.Paging.Next);
            animeList.Data.AddRange(nextAnimeList.Data);
        }

        return animeList;
    }

    public async Task<List<Theme>> GetAnimeListThemeSongsAsync(List<int> animeIds)
    {
        MenuController.Instance.UpdateProgressBar(0, $"Loading...");

        List<Theme> themeSongs = new();
        List<AnimeField> fields = new() { AnimeField.OpeningThemes, AnimeField.EndingThemes };

        foreach (int animeId in animeIds)
        {
            AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(animeId, fields);
            await Task.Delay(requestDelay);

            if (animeDetails != null)
            {
                MenuController.Instance.UpdateProgressBar(0, animeDetails.Title);

                if (animeDetails.OpeningThemes != null)
                {
                    foreach (Theme themeSong in animeDetails.OpeningThemes)
                    {
                        themeSong.SongInfo = StringManipulator.ExtractSongInfo(themeSong.Text);
                        themeSongs.Add(themeSong);
                    }
                }
            }
        }

        return themeSongs;
    }

    private void SaveThemeSongList(Dictionary<int, Theme> themeSongList)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{SongListSaveName}.{SaveFileExtension}");
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string serializedList = JsonSerializer.Serialize(themeSongList, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
    }

    private Dictionary<int, Theme> LoadThemeSongList()
    {
        string serializedList = GetSerializedSongList();
        if (serializedList == null) return new();

        Dictionary<int, Theme> savedList = JsonSerializer.Deserialize<Dictionary<int, Theme>>(serializedList);
        return savedList;
    }

    private void SaveUpdatedAt(AnimeListResponse animeList)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{UpdatedAtSaveName}.{SaveFileExtension}");
        Dictionary<int, string> updatedAt = new();

        foreach (AnimeListNode node in animeList.Data)
        {
            updatedAt[node.Node.Id] = node.Node.UpdatedAt;
        }

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string serializedList = JsonSerializer.Serialize(updatedAt, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
    }

    private Dictionary<int, string> LoadUpdatedAt()
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{UpdatedAtSaveName}.{SaveFileExtension}");
        if (!File.Exists(savedListPath)) return new();

        string serializedUpdatedAt = File.ReadAllText(savedListPath);
        Dictionary<int, string> updatedAt = JsonSerializer.Deserialize<Dictionary<int, string>>(serializedUpdatedAt);

        return updatedAt;
    }

    private List<int> CheckForUpdates(AnimeListResponse newAnimeList)
    {
        Dictionary<int, string> cachedUpdatedAt = LoadUpdatedAt();
        List<int> updatedAnime = new();

        foreach (AnimeListNode node in newAnimeList.Data)
        {
           if (!cachedUpdatedAt.TryGetValue(node.Node.Id, out string cachedTime) || cachedTime != node.Node.UpdatedAt)
            {
                updatedAnime.Add(node.Node.Id);
            }
        }

        SaveUpdatedAt(newAnimeList);

        return updatedAnime;
    }

    public string GetSerializedSongList()
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{SongListSaveName}.{SaveFileExtension}");
        if (!File.Exists(savedListPath)) return null;

        string serializedList = File.ReadAllText(savedListPath);
        return serializedList;
    }
}
