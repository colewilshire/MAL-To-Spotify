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
    [SerializeField] private Button seekAnimeButton;
    private MALClient malClient;
    private Dictionary<int, Theme>.Enumerator enumerator;
    private AnimeListResponse fullAnimeList;
    //private int currentAnimeIndex = 0;
    private readonly int requestDelay = 1100;
    public Dictionary<int, Theme> OpeningThemes;
    public string SongListSaveName = "OpeningThemes";
    public string SaveFileExtension = "txt";

    private void Start()
    {
        malLoginButton.onClick.AddListener(async () => await OnMalLogin());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task OnMalLogin()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);

        malClient = await AuthenticationController.Instance.AuthenticateMALClient();
        OpeningThemes = LoadThemeSongList(SongListSaveName);
        malInputField.text = (await malClient.GetMyUserInfoAsync()).Name;

        while (fullAnimeList == null)
        {
            fullAnimeList = await GetAnimeListAsync();
        }

        List<int> updatedAnime = CheckForUpdates(fullAnimeList);
        Debug.Log("abc");
        if (updatedAnime.Count > 0)
        {
            Debug.Log("def");
            List<Theme> allThemes = new();

            //while (currentAnimeIndex < fullAnimeList.Data.Count)
            //{
            List<Theme> tempOpeningThemes = await GetAnimeListThemeSongsAsync(updatedAnime);
            allThemes.AddRange(tempOpeningThemes);
            //}

            OpeningThemes.AddRange(allThemes
                .GroupBy(theme => theme.Id)
                .ToDictionary(group => group.Key, group => group.First()));

            SaveThemeSongList(OpeningThemes, SongListSaveName);
        }

        // if (OpeningThemes == null)
        // {
        //     while (fullAnimeList == null)
        //     {
        //         fullAnimeList = await GetAnimeListAsync();
        //     }

        //     List<Theme> allThemes = new();

        //     while (currentAnimeIndex < fullAnimeList.Data.Count)
        //     {
        //         List<Theme> tempOpeningThemes = await GetAnimeListThemeSongsAsync(fullAnimeList, currentAnimeIndex);
        //         allThemes.AddRange(tempOpeningThemes);
        //     }

        //     OpeningThemes = allThemes
        //         .GroupBy(theme => theme.Id)
        //         .ToDictionary(group => group.Key, group => group.First());

        //     SaveThemeSongList(OpeningThemes, SongListSaveName);
        // }

        // if (OpeningThemes != null)
        // {
        enumerator = OpeningThemes.GetEnumerator();
        // }

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

        //SaveUpdatedAt(animeList, SongListSaveName);
        //CheckForUpdates(animeList);

        return animeList;
    }

    public async Task<List<Theme>> GetAnimeListThemeSongsAsync(List<int> animeIds)//, int startingIndex = 0)
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

            //currentAnimeIndex++;
        }

        // for (int i = startingIndex; i < animeList.Data.Count; ++i)
        // {
        //     AnimeListNode anime = animeList.Data[i];
        //     AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(anime.Node.Id, fields);
        //     await Task.Delay(requestDelay);

        //     if (animeDetails != null)
        //     {
        //         MenuController.Instance.UpdateProgressBar(i / animeList.Data.Count, animeDetails.Title);

        //         if (animeDetails.OpeningThemes != null)
        //         {
        //             foreach (Theme themeSong in animeDetails.OpeningThemes)
        //             {
        //                 themeSong.SongInfo = StringManipulator.ExtractSongInfo(themeSong.Text);
        //                 themeSongs.Add(themeSong);
        //             }
        //         }
        //     }

        //     currentAnimeIndex++;
        // }

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
        if (serializedList == null) return new();

        Dictionary<int, Theme> savedList = JsonSerializer.Deserialize<Dictionary<int, Theme>>(serializedList);
        return savedList;
    }

    private void SaveUpdatedAt(AnimeListResponse animeList, string saveName)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}UpdatedAt.{SaveFileExtension}");
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
        //
        //DebugController.Instance.ExportUpdatedAt(updatedAt, saveName, SaveFileExtension);
        //

        // foreach (AnimeListNode node in animeList.Data)
        // {
        //     Debug.Log(node.Node.UpdatedAt);
        // }

        // Debug.Log($"Id: {animeList.Data[0].Node.Id == null}");
        // Debug.Log($"Title: {animeList.Data[0].Node.Title == null}");
        // Debug.Log($"MainPicture: {animeList.Data[0].Node.MainPicture == null}");
        // Debug.Log($"AlternativeTitles: {animeList.Data[0].Node.AlternativeTitles == null}");
        // Debug.Log($"StartDate: {animeList.Data[0].Node.StartDate == null}");
        // Debug.Log($"EndDate: {animeList.Data[0].Node.EndDate == null}");
        // Debug.Log($"Synopsis: {animeList.Data[0].Node.Synopsis == null}");
        // Debug.Log($"Mean: {animeList.Data[0].Node.Mean == null}");
        // Debug.Log($"Rank: {animeList.Data[0].Node.Rank == null}");
        // Debug.Log($"Popularity: {animeList.Data[0].Node.Popularity == null}");
        // Debug.Log($"NumListUsers: {animeList.Data[0].Node.NumListUsers == null}");
        // Debug.Log($"NumScoringUsers: {animeList.Data[0].Node.NumScoringUsers == null}");
        // Debug.Log($"Nsfw: {animeList.Data[0].Node.Nsfw == null}");
        // Debug.Log($"CreatedAt: {animeList.Data[0].Node.CreatedAt == null}");
        // Debug.Log($"UpdatedAt: {animeList.Data[0].Node.UpdatedAt == null}");
        // Debug.Log($"MediaType: {animeList.Data[0].Node.MediaType == null}");
        // Debug.Log($"Status: {animeList.Data[0].Node.Status == null}");
        // Debug.Log($"Genres: {animeList.Data[0].Node.Genres == null}");
        // Debug.Log($"MyListStatus: {animeList.Data[0].Node.MyListStatus == null}");
        // Debug.Log($"NumEpisodes: {animeList.Data[0].Node.NumEpisodes == null}");
        // Debug.Log($"StartSeason: {animeList.Data[0].Node.StartSeason == null}");
        // Debug.Log($"Broadcast: {animeList.Data[0].Node.Broadcast == null}");
        // Debug.Log($"Source: {animeList.Data[0].Node.Source == null}");
        // Debug.Log($"AverageEpisodeDuration: {animeList.Data[0].Node.AverageEpisodeDuration == null}");
        // Debug.Log($"Rating: {animeList.Data[0].Node.Rating == null}");
        // Debug.Log($"Pictures: {animeList.Data[0].Node.Pictures == null}");  // true
        // Debug.Log($"Background: {animeList.Data[0].Node.Background == null}");  // true
        // Debug.Log($"Related Anime: {animeList.Data[0].Node.RelatedAnime == null}");  // true
        // Debug.Log($"Recommendations: {animeList.Data[0].Node.Recommendations == null}");  // true
        // Debug.Log($"Studios: {animeList.Data[0].Node.Studios == null}");
        // Debug.Log($"Statistics: {animeList.Data[0].Node.Statistics == null}");  // true
        // Debug.Log($"Opening Themes: {animeList.Data[0].Node.OpeningThemes == null}");  // true
        // Debug.Log($"Ending Themes: {animeList.Data[0].Node.EndingThemes == null}");  // true
    }

    // private void UpdateUpdatedAt(Dictionary<int, string> newUpdatedAt, string saveName)
    // {
    //     string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}UpdatedAt.{SaveFileExtension}");

    //     JsonSerializerOptions jsonSerializerOptions = new()
    //     {
    //         WriteIndented = true,
    //         Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //     };
    //     string serializedList = JsonSerializer.Serialize(newUpdatedAt, jsonSerializerOptions);

    //     File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
    // }

    private Dictionary<int, string> LoadUpdatedAt(string saveName)
    {
        Debug.Log("x");
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}UpdatedAt.{SaveFileExtension}");
        if (!File.Exists(savedListPath)) return new();
        Debug.Log("y");

        string serializedUpdatedAt = File.ReadAllText(savedListPath);
        Dictionary<int, string> updatedAt = JsonSerializer.Deserialize<Dictionary<int, string>>(serializedUpdatedAt);

        return updatedAt;
    }

    private List<int> CheckForUpdates(AnimeListResponse newAnimeList)
    {
        Dictionary<int, string> cachedUpdatedAt = LoadUpdatedAt(SongListSaveName);
        Debug.Log("z");
        List<int> updatedAnime = new();

        foreach (AnimeListNode node in newAnimeList.Data)
        {
           if (!cachedUpdatedAt.TryGetValue(node.Node.Id, out string cachedTime) || cachedTime != node.Node.UpdatedAt)
            {
                updatedAnime.Add(node.Node.Id);
            }
        }

        SaveUpdatedAt(newAnimeList, SongListSaveName);

        return updatedAnime;
    }

    public string GetSerializedSongList(string saveName)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{SaveFileExtension}");
        if (!File.Exists(savedListPath)) return null;

        string serializedList = File.ReadAllText(savedListPath);
        return serializedList;
    }
}
