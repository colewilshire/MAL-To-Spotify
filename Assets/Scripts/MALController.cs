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

        Debug.Log(StringManipulator.Test());
    }

    private async Task Test()
    {
        malLoginButton.interactable = false;
        spotifyLoginButton.interactable = false;
        seekAnimeButton.interactable = false;

        //malClient = await AuthenticationController.Instance.AuthenticateMALClient();
        OpeningThemes = LoadThemeSongList(SongListSaveName);
        GetStats();
        //
        return;

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
                        themeSong.SongInfo = StringManipulator.ExtractSongInfo(themeSong.Text);
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

    public void GetStats()
    {
        int titleMismatches = 0;
        int artistMismatches = 0;

        foreach (KeyValuePair<int, Theme> kvp in OpeningThemes)
        {
            bool titleFound = false;
            
            foreach (string title in kvp.Value.SongInfo.MALSongInfo.Titles)
            {
                if (title != null && kvp.Value.SongInfo.SpotifySongInfo.Title != null)  //
                {
                    string processedMALTitle = StringManipulator.ProcessString(title);
                    string processedSpotifyTitle = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Title);

                    if (StringManipulator.CompareStrings(processedMALTitle, processedSpotifyTitle))
                    {
                        titleFound = true;
                    }
                    else if (StringManipulator.CompareHiragana(processedMALTitle, processedSpotifyTitle))
                    {
                        //Debug.Log(title);
                        titleFound = true;
                    }
                }
            }

            if (titleFound == false)
            {
                titleMismatches++;

                string processedStr1 = StringManipulator.ProcessString(kvp.Value.SongInfo.MALSongInfo.Titles[0]);
                string processedStr2 = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Title);
                Debug.Log($"{processedStr1} : {processedStr2}");
            }

            // // Check if index 0 matches
            // if (kvp.Value.SongInfo.MALSongInfo.Titles[0].Trim('"') == kvp.Value.SongInfo.SpotifySongInfo.Title)
            // {
                
            // }
            // // Chcek if in
            // else if (kvp.Value.SongInfo.MALSongInfo.Titles.Count > 1 && kvp.Value.SongInfo.MALSongInfo.Titles[1] == kvp.Value.SongInfo.SpotifySongInfo.Title)
            // {

            // }
            // else
            // {
            //     Debug.Log($"{kvp.Value.SongInfo.MALSongInfo.Titles[0].Trim('"')} : {kvp.Value.SongInfo.SpotifySongInfo.Title}");
            //     titleMismatches++;
            // }

            if (StringManipulator.CompareStrings(kvp.Value.SongInfo.MALSongInfo.Artists[0], kvp.Value.SongInfo.SpotifySongInfo.Artist) == false)
            {
                //Debug.Log($"{malArtist} : {kvp.Value.SongInfo.SpotifySongInfo.Artist}");
                artistMismatches++;
            }
        }

        float a = (float) titleMismatches/OpeningThemes.Count*100;
        float b = (float) artistMismatches/OpeningThemes.Count*100;

        Debug.Log($"Title: {titleMismatches}: {a}%"); // 67.65% // 61.41%      //51.97% //31.3% //30.71%    //29.72%
        Debug.Log($"Artist: {artistMismatches}: {b}%"); // 55.22%   // 56.89%      //56.89 //46.26%
    }
}
