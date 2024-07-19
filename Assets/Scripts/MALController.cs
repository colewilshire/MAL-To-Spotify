using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class MALController : MonoBehaviour
{
    [SerializeField] private TMP_InputField malInputField;
    [SerializeField] private Button malLoginButton;
    [SerializeField] private Button seekAnimeButton;

    private AuthenticationController authenticationController;
    private MALClient malClient;

    private int ind = 0;
    AnimeListResponse animeList;

    private void Start()
    {
        authenticationController = GetComponent<AuthenticationController>();

        malLoginButton.onClick.AddListener(async () => await Test());
        seekAnimeButton.onClick.AddListener(SeekAnime);
    }

    private async Task Test()
    {
        malClient = await authenticationController.AuthenticateMALClient();

        // await TestGetAnimeListAsync();
        // await TestGetAnimeDetailsAsync(1);
        // await TestGetAnimeRankingAsync();
        // await TestGetSeasonalAnimeAsync(2021, "spring");
        // await TestGetSuggestedAnimeAsync();
        // await TestGetMyUserInfoAsync();

        animeList = await GetAnimeListAsync();
        //await GetRawAnimeListAsync();
    }

    private void SeekAnime()
    {
        if (animeList == null || ind >= animeList.Data.Count) return;

        ++ind;
        malInputField.text = $"{ind}. {animeList.Data[ind].Node.Title}";
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

    public async Task<AnimeListResponse> GetAnimeListAsync()
    {
        AnimeListResponse animeList = await malClient.GetAnimeListAsync();

        //var q = animeList.Data[1];

        //string l = "";
        // foreach (AnimeListNode anime in animeList.Data)
        // {
        //     malInputField.text = anime.Node.Title;
        //     //l += $"{count}. {anime.Node.Title}\n";
        // }
        //malInputField.text = $"{animeList.Data.Count.ToString()}, {animeList.Paging.Next}";
        //malInputField.text = l;
        //malInputField.text = animeList.Paging.Next;

        malInputField.text = $"0. {animeList.Data[0].Node.Title}";
        //malInputField.text = $"{animeList.Paging.Previous}";

        return animeList;
    }
}