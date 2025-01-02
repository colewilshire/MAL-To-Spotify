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

public class MALController : Singleton<MALController>
{
    [SerializeField] private TMP_InputField malInputField;
    [SerializeField] private Button malLoginButton;
    [SerializeField] private Button spotifyLoginButton;
    [SerializeField] private Button seekAnimeButton;
    private MALClient malClient;
    private Dictionary<int, Theme>.Enumerator enumerator;
    private AnimeListResponse fullAnimeList;
    public Dictionary<int, Theme> OpeningThemes;
    private int currentAnimeIndex = 0;
    private int iteration = 0;
    public string SongListSaveName = "OpeningThemes";
    public string SaveFileExtension = "json";

    private void Start()
    {
        malLoginButton.onClick.AddListener(async () => await Test());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task Test()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);

        malClient = await AuthenticationController.Instance.AuthenticateMALClient();
        OpeningThemes = LoadThemeSongList(SongListSaveName);
        malInputField.text = (await malClient.GetMyUserInfoAsync()).Name;

        if (OpeningThemes == null)
        {
            while (fullAnimeList == null)
            {
                fullAnimeList = await GetAnimeListAsync();
            }

            List<Theme> allThemes = new();

            while (currentAnimeIndex < fullAnimeList.Data.Count)
            {
                List<Theme> tempOpeningThemes = await GetAnimeListThemeSongsAsync(fullAnimeList, currentAnimeIndex);
                allThemes.AddRange(tempOpeningThemes);
            }

            OpeningThemes = allThemes
                .GroupBy(theme => theme.Id)
                .ToDictionary(group => group.Key, group => group.First());

            SaveThemeSongList(OpeningThemes, SongListSaveName);
        }

        if (OpeningThemes != null)
        {
            enumerator = OpeningThemes.GetEnumerator();
        }

        malLoginButton.interactable = false;
        spotifyLoginButton.interactable = true;
        SeekAnime();
        MenuController.Instance.SetMenu(MenuState.Main);
    }

    private void SeekAnime()
    {
        bool success = enumerator.MoveNext();

        if (success)
        {
            KeyValuePair<int, Theme> current = enumerator.Current;
            MenuController.Instance.UpdateProgressBar(0, $"{current.Key}. {current.Value.Text}");
        }
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
            animeList = await malClient.GetAnimeListAsync();
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

    public async Task<List<Theme>> GetAnimeListThemeSongsAsync(AnimeListResponse animeList, int startingIndex = 0)
    {
        iteration++;
        MenuController.Instance.UpdateProgressBar(0, $"Loading... [{iteration}]");

        List<Theme> themeSongs = new();
        List<AnimeField> fields = new() { AnimeField.OpeningThemes, AnimeField.EndingThemes };

        for (int i = startingIndex; i < animeList.Data.Count; ++i)
        {
            AnimeListNode anime = animeList.Data[i];
            AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(anime.Node.Id, fields);

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

            currentAnimeIndex++;
        }

        return themeSongs;
    }

    private void SaveThemeSongList(Dictionary<int, Theme> themeSongList, string saveName)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{SaveFileExtension}");

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string serializedList = JsonSerializer.Serialize(themeSongList, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
    }

    private Dictionary<int, Theme> LoadThemeSongList(string saveName)
    {
        string serializedList = GetSerializedSongList(saveName);
        if (serializedList == null) return null;

        Dictionary<int, Theme> savedList = JsonSerializer.Deserialize<Dictionary<int, Theme>>(serializedList);
        return savedList;
    }

    public string GetSerializedSongList(string saveName)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{SaveFileExtension}");
        if (!File.Exists(savedListPath)) return null;

        string serializedList = File.ReadAllText(savedListPath);
        return serializedList;
    }
}
