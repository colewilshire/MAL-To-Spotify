using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class MALClient
{
    private readonly HttpClient httpClient;
    private readonly string accessToken;
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";

    public MALClient(string accessToken)
    {
        this.httpClient = new HttpClient();
        this.accessToken = accessToken;
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    // Get the list of anime for a user
    public async Task<AnimeListResponse> GetAnimeListAsync(string userName, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}users/{userName}/animelist?{fieldsQuery}";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnimeListResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve anime list. Status code: {response.StatusCode}");
        }
    }


    public async Task<AnimeDetails> GetAnimeDetailsAsync(int animeId, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : "";
        string url = $"{baseUrl}anime/{animeId}?{fieldsQuery}";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            AnimeDetails animeDetails = JsonSerializer.Deserialize<AnimeDetails>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return animeDetails;
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve anime details. Status code: {response.StatusCode}");
        }
    }

    // Get anime ranking
    public async Task<AnimeRankingResponse> GetAnimeRankingAsync(string rankingType = "all", int limit = 10, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}anime/ranking?ranking_type={rankingType}&limit={limit}&{fieldsQuery}";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnimeRankingResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve anime ranking. Status code: {response.StatusCode}");
        }
    }

    // Get suggested anime (this might require special permissions or scopes)
    public async Task<AnimeSuggestionResponse> GetSuggestedAnimeAsync()
    {
        string url = $"{baseUrl}anime/suggestions";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AnimeSuggestionResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve suggested anime. Status code: {response.StatusCode}");
        }
    }


    // Get anime ranking
    public async Task<SeasonalAnimeResponse> GetSeasonalAnimeAsync(int year, string season, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}anime/season/{year}/{season}?{fieldsQuery}";

        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SeasonalAnimeResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            throw new HttpRequestException($"Failed to retrieve seasonal anime. Status code: {response.StatusCode}");
        }
    }
}
