using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;
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

    [SerializeField] private string saveFileExtension = "sav";
    public string SongListSaveName = "OpeningThemes";

    private void Start()
    {
        malLoginButton.onClick.AddListener(async () => await Test());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task Test()
    {
        malLoginButton.interactable = false;
        spotifyLoginButton.interactable = false;
        seekAnimeButton.interactable = false;

        malClient = await AuthenticationController.Instance.AuthenticateMALClient();
        OpeningThemes = LoadThemeSongList(SongListSaveName);

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

        SeekAnime();
        seekAnimeButton.interactable = true;
        malLoginButton.interactable = true;
        spotifyLoginButton.interactable = true;
    }


    private void SeekAnime()
    {
        bool success = enumerator.MoveNext();

        if (success)
        {
            KeyValuePair<int, Theme> current = enumerator.Current;
            malInputField.text = $"{current.Key}. {current.Value.Text}";
        }
    }

    private async Task TestGetAnimeListAsync()
    {
        AnimeListResponse animeList = await malClient.GetAnimeListAsync("@me");

        Debug.Log($"First anime in list: {animeList.Data[0].Node.Title}");
        malInputField.text = $"First anime in list: {animeList.Data[0].Node.Title}";
    }

    private async Task TestGetAnimeDetailsAsync(int animeId)
    {
        List<AnimeField> fields = AnimeFieldExtensions.GetAllFields();
        AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(animeId, fields);

        string detailsOutput = $"Anime title: {animeDetails.Title}\nID: {animeDetails.Id}\nGenres: {animeDetails.Genres}\nSynopsis: {animeDetails.Synopsis}\n Score: {animeDetails.Mean}\nNumber of Episodes: {animeDetails.NumEpisodes}\nStatus: {animeDetails.Status}";
        Debug.Log(detailsOutput);
        malInputField.text = detailsOutput;
    }

    private async Task TestGetAnimeRankingAsync()
    {
        AnimeRankingResponse animeRanking = await malClient.GetAnimeRankingAsync();

        Debug.Log($"Top ranked anime: {animeRanking.Data[0].Node.Title}");
        malInputField.text = $"Top ranked anime: {animeRanking.Data[0].Node.Title}";
    }

    private async Task TestGetSeasonalAnimeAsync(int year, string season)
    {
        SeasonalAnimeResponse seasonalAnime = await malClient.GetSeasonalAnimeAsync(year, season);

        Debug.Log($"Seasonal anime: {seasonalAnime.Data[0].Node.Title}");
        malInputField.text = $"Seasonal anime: {seasonalAnime.Data[0].Node.Title}";
    }

    private async Task TestGetSuggestedAnimeAsync()
    {
        AnimeSuggestionResponse suggestedAnime = await malClient.GetSuggestedAnimeAsync();

        Debug.Log($"Suggested anime: {suggestedAnime.Data[0].Node.Title}");
        malInputField.text = $"Suggested anime: {suggestedAnime.Data[0].Node.Title}";
    }

    private async Task TestGetMyUserInfoAsync()
    {
        UserInfoResponse myUserInfo = await malClient.GetMyUserInfoAsync(new List<UserInfoField>() {UserInfoField.Id});
        string userInfoOutput = $"User Info: \n" +
                                $"ID: {myUserInfo.Id}\n" +
                                $"Name: {myUserInfo.Name}\n" +
                                $"Picture: {myUserInfo.Picture}\n" +
                                $"Gender: {myUserInfo.Gender}\n" +
                                $"Birthday: {myUserInfo.Birthday}\n" +
                                $"Location: {myUserInfo.Location}\n" +
                                $"Joined At: {myUserInfo.JoinedAt}\n" +
                                $"Anime Statistics: {myUserInfo.AnimeStatistics}\n" +
                                $"Time Zone: {myUserInfo.TimeZone}\n" +
                                $"Is Supporter: {myUserInfo.IsSupporter}";

        Debug.Log(userInfoOutput);
        malInputField.text = userInfoOutput;
    }

    public async Task<AnimeListResponse> GetAnimeListAsync(string nextPageUrl = null)
    {
        AnimeListResponse animeList;

        malInputField.text = $"Loading...";

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
            malInputField.text = $"Returning...";
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
        malInputField.text = $"Loading... [{iteration}]";

        List<Theme> themeSongs = new();
        List<AnimeField> fields = new() { AnimeField.OpeningThemes };

        for (int i = startingIndex; i < animeList.Data.Count; ++i)
        {
            AnimeListNode anime = animeList.Data[i];
            AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(anime.Node.Id, fields);

            if (animeDetails != null)
            {
                malInputField.text = animeDetails.Title;

                if (animeDetails.OpeningThemes != null)
                {
                    foreach (Theme themeSong in animeDetails.OpeningThemes)
                    {
                        themeSong.SongInfo = ExtractSongInfo(themeSong.Text);
                        themeSongs.Add(themeSong);
                    }
                }
            }
            else
            {
                malInputField.text = $"Iteration: {iteration}, {i}. null";
                break;
            }

            currentAnimeIndex++;
        }

    return themeSongs;
    }

    private void SaveThemeSongList(Dictionary<int, Theme> themeSongList, string saveName)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{saveFileExtension}");

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true
        };
        string serializedList = JsonSerializer.Serialize(themeSongList, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList);
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
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{saveFileExtension}");
        if (!File.Exists(savedListPath)) return null;

        string serializedList = File.ReadAllText(savedListPath);
        return serializedList;
    }

    public void ExportSongList()
    {
        SaveThemeSongList(OpeningThemes, SongListSaveName);

        string savedListPath = Path.Combine(Application.persistentDataPath, $"{SongListSaveName}.{saveFileExtension}");
        if (!File.Exists(savedListPath)) return;

        NativeFilePicker.ExportFile(savedListPath);
    }

    public static SongInfo ExtractSongInfo(string input)
    {
        // Remove initial pattern "#number:" or "#number" and trim
        string pattern = @"^#\d+:?\s*";
        string cleanedInput = Regex.Replace(input, pattern, "").Trim();

        // Split the string at the first occurrence of " by " into title and artist
        int byIndex = cleanedInput.IndexOf(" by ", StringComparison.OrdinalIgnoreCase);
        if (byIndex > -1)
        {
            string title = cleanedInput.Substring(0, byIndex).Trim();
            string artist = cleanedInput.Substring(byIndex + 4).Trim();

            // Return SongInfo object with extracted title and artist
            SongInfo songInfo = new()
            {
                MALSongInfo = new()
                {
                    Titles = SplitString(title),
                    Artists = SplitString(artist)
                },
                SpotifySongInfo = new()
            };

            songInfo.SpotifySongInfo.Query = $"{songInfo.MALSongInfo.Titles[0]} {songInfo.MALSongInfo.Artists[0]}";

            return songInfo;
        }

        // Return null if the string does not match the expected format
        return null;
    }

    private static List<string> SplitString(string input)
    {
        List<string> parts = new();
        string pattern = @"(.*?)\s*\((.*?)\)";
        Regex regex = new(pattern);
        MatchCollection matches = regex.Matches(input);
        bool foundOutsideParentheses = false;

        foreach (Match match in matches)
        {
            string outsideParentheses = match.Groups[1].Value.Trim();
            string insideParentheses = match.Groups[2].Value.Trim();

            if (!string.IsNullOrEmpty(outsideParentheses))
            {
                parts.Add(outsideParentheses);
                foundOutsideParentheses = true;
            }
            if (!string.IsNullOrEmpty(insideParentheses) && !insideParentheses.StartsWith("ep", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(insideParentheses);
            }
        }

        if (!foundOutsideParentheses && matches.Count == 0)
        {
            parts.Add(input.Trim());
        }

        return parts;
    }

    // public void GetStats()
    // {
    //     int titleMismatches = 0;
    //     int artistMismatches = 0;
    //     int queryMismatches = 0;

    //     foreach (KeyValuePair<int, Theme> kvp in OpeningThemes)
    //     {
    //         string s = $"{kvp.Value.SpotifyName} {kvp.Value.SpotifyArtist}";
    //         string malArtist = $"{kvp.Value.MalArtist}".Trim();

    //         if (kvp.Value.MalName != kvp.Value.SpotifyName)
    //         {
    //             //Debug.Log($"{kvp.Value.MalName} : {kvp.Value.SpotifyName}");
    //             titleMismatches++;
    //         }

    //         if (malArtist != kvp.Value.SpotifyArtist)
    //         {
    //             Debug.Log($"{malArtist} : {kvp.Value.SpotifyArtist}");
    //             artistMismatches++;
    //         }

    //         if (s != kvp.Value.SpotifyQuery)
    //         {
    //             queryMismatches++;
    //         }
    //     }

    //     float a = (float) titleMismatches/OpeningThemes.Count*100;
    //     float b = (float) artistMismatches/OpeningThemes.Count*100;
    //     float c = (float) queryMismatches/OpeningThemes.Count*100;

    //     Debug.Log($"Title: {titleMismatches}: {a}%"); // 67.65%
    //     Debug.Log($"Artist: {artistMismatches}: {b}%"); // 55.22%
    //     Debug.Log($"Query: {queryMismatches}: {c}%");
    // }
}
