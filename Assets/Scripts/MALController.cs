using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MALController : MonoBehaviour
{
    [SerializeField] private TMP_InputField malInputField;
    [SerializeField] private Button malLoginButton;
    [SerializeField] private Button seekAnimeButton;

    private AuthenticationController authenticationController;
    private MALClient malClient;

    private int ind = 0;
    private AnimeListResponse fullAnimeList;
    private readonly List<Theme> openingThemes = new();

    private int currentAnimeIndex = 0;
    private int iteration = 0;

    private void Start()
    {
        authenticationController = GetComponent<AuthenticationController>();

        malLoginButton.onClick.AddListener(async () => await Test());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task Test()
    {
        malLoginButton.interactable = false;
        seekAnimeButton.interactable = false;

        malClient = await authenticationController.AuthenticateMALClient();

        while (fullAnimeList == null)
        {
            fullAnimeList = await GetAnimeListAsync();
        }

        if (fullAnimeList != null)
        {
            while (currentAnimeIndex < fullAnimeList.Data.Count)
            {
                List<Theme> tempOpeningThemes = await GetAnimeListThemeSongsAsync(fullAnimeList, currentAnimeIndex);
                openingThemes.AddRange(tempOpeningThemes);
            }

            if (openingThemes != null && openingThemes.Count > 0 && openingThemes[0] != null)
            {
                string t = openingThemes[0].Text;
                malInputField.text = $"{0}. {t}";
            }
        }

        SeekAnime();
        seekAnimeButton.interactable = true;
        malLoginButton.interactable = true;
    }

    private void SeekAnime()
    {
        if (fullAnimeList != null && ind < openingThemes.Count)
        {
            string t = openingThemes[ind].Text;
            malInputField.text = $"{ind}. {t}";
            ++ind;
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

        ind = 0;
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
}