using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MALController : MonoBehaviour
{
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";
    private readonly HttpClient httpClient = new();
    private MALAuthenticator malAuthenticator;

    [SerializeField] private Button button2;
    [SerializeField] private TMP_Text anime;
    public AnimeDetails animeDetails;

    public MALClient MALClient;

    private void Start()
    {
        malAuthenticator = GetComponent<MALAuthenticator>();

        button2.onClick.AddListener(Test);
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
        var animeList = await MALClient.GetAnimeListAsync("@me");
        Debug.Log($"First anime in list: {animeList.Data[0].Node.Title}");
    }

    private async Task TestGetAnimeDetailsAsync(int animeId)
    {
        var animeDetails = await MALClient.GetAnimeDetailsAsync(animeId);
        Debug.Log($"Anime title: {animeDetails.Title}, Synopsis: {animeDetails.Synopsis}");
    }

    private async Task TestGetAnimeRankingAsync()
    {
        var animeRanking = await MALClient.GetAnimeRankingAsync();
        Debug.Log($"Top ranked anime: {animeRanking.Data[0].Node.Title}");
    }

    private async Task TestGetSeasonalAnimeAsync(int year, string season)
    {
        var seasonalAnime = await MALClient.GetSeasonalAnimeAsync(year, season);
        Debug.Log($"Seasonal anime: {seasonalAnime.Data[0].Node.Title}");
    }

    private async Task TestGetSuggestedAnimeAsync()
    {
        var suggestedAnime = await MALClient.GetSuggestedAnimeAsync();
        Debug.Log($"Suggested anime: {suggestedAnime.Data[0].Node.Title}");
    }
}