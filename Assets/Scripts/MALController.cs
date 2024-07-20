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
    private List<Theme> openingThemes;

    private void Start()
    {
        authenticationController = GetComponent<AuthenticationController>();

        malLoginButton.onClick.AddListener(async () => await Test());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task Test()
    {
        malClient = await authenticationController.AuthenticateMALClient();

        fullAnimeList = await GetAnimeListAsync();
        //seekAnimeButton.interactable = true;

        //openingThemes = await GetAnimeListThemeSongsAsync(fullAnimeList);
        seekAnimeButton.interactable = true;
    }

    private void SeekAnime()
    {
        if (fullAnimeList == null || ind >= fullAnimeList.Data.Count) return;

        ++ind;
        malInputField.text = $"{ind}. {fullAnimeList.Data[ind].Node.Title}";

        // if (fullAnimeList == null || ind >= openingThemes.Count) return;
        // ++ind;
        // malInputField.text = $"{ind}. {openingThemes[ind].Text}";
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
        //AnimeListResponse animeList;
        AnimeListResponse animeList = new();

        malLoginButton.interactable = false;
        seekAnimeButton.interactable = false;
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

        if (animeList.Paging.Next != null)
        {
            AnimeListResponse nextAnimeList = await GetAnimeListAsync(animeList.Paging.Next);
            animeList.Data.AddRange(nextAnimeList.Data);
        }

        malInputField.text = $"0. {animeList.Data[0].Node.Title}";
        //malInputField.text = await malClient.GetRawAnimeListAsync("@me", new List<AnimeField>(){AnimeField.OpeningThemes}, true, 0, 2);
        //malInputField.text = await malClient.GetRawAnimeDetailsAsync(1, new List<AnimeField>(){AnimeField.OpeningThemes});
        malLoginButton.interactable = true;

        return animeList;
    }

    public async Task<List<Theme>> GetAnimeListThemeSongsAsync(AnimeListResponse animeList)
    {
        List<Theme> themeSongs = new();
        List<AnimeField> fields = new() {AnimeField.OpeningThemes};

        foreach (AnimeListNode anime in animeList.Data)
        {
            AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(anime.Node.Id, fields);

            malInputField.text = $"{anime.Node.Title}";

            if (animeDetails.OpeningThemes != null)
            {
                themeSongs.AddRange(animeDetails.OpeningThemes);
            }
        }

        malInputField.text = $"0. {themeSongs[0].Text}";
        malLoginButton.interactable = true;

        return themeSongs;
    }
}