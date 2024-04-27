using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MALController : MonoBehaviour
{
    [SerializeField] private TMP_Text anime;
    private MALAuthenticator malAuthenticator;
    private MALClient malClient;
    public AnimeDetails animeDetails;

    private async void Start()
    {
        malAuthenticator = GetComponent<MALAuthenticator>();

        malClient = await malAuthenticator.AuthenticateMALClient();
        Test();
    }

    private async void Test()
    {
        await TestGetAnimeListAsync();
        await TestGetAnimeDetailsAsync(1);
        await TestGetAnimeRankingAsync();
        await TestGetSeasonalAnimeAsync(2021, "spring");
        await TestGetSuggestedAnimeAsync();
    }

    private async Task TestGetAnimeListAsync()
    {
        AnimeListResponse animeList = await malClient.GetAnimeListAsync("@me");

        Debug.Log($"First anime in list: {animeList.Data[0].Node.Title}");
        anime.text = $"First anime in list: {animeList.Data[0].Node.Title}";
    }

    private async Task TestGetAnimeDetailsAsync(int animeId)
    {
        AnimeDetails animeDetails = await malClient.GetAnimeDetailsAsync(animeId);

        Debug.Log($"Anime title: {animeDetails.Title}, Synopsis: {animeDetails.Synopsis}");
        anime.text = $"Anime title: {animeDetails.Title}, Synopsis: {animeDetails.Synopsis}";
    }

    private async Task TestGetAnimeRankingAsync()
    {
        AnimeRankingResponse animeRanking = await malClient.GetAnimeRankingAsync();

        Debug.Log($"Top ranked anime: {animeRanking.Data[0].Node.Title}");
        anime.text = $"Top ranked anime: {animeRanking.Data[0].Node.Title}";
    }

    private async Task TestGetSeasonalAnimeAsync(int year, string season)
    {
        SeasonalAnimeResponse seasonalAnime = await malClient.GetSeasonalAnimeAsync(year, season);

        Debug.Log($"Seasonal anime: {seasonalAnime.Data[0].Node.Title}");
        anime.text = $"Seasonal anime: {seasonalAnime.Data[0].Node.Title}";
    }

    private async Task TestGetSuggestedAnimeAsync()
    {
        AnimeSuggestionResponse suggestedAnime = await malClient.GetSuggestedAnimeAsync();

        Debug.Log($"Suggested anime: {suggestedAnime.Data[0].Node.Title}");
        anime.text = $"Suggested anime: {suggestedAnime.Data[0].Node.Title}";
    }
}