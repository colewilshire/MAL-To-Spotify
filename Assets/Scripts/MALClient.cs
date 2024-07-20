using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

public class MALClient
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";

    public MALClient(string accessToken)
    {
        httpClient = new();
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
    }

    private async Task<T> FetchDataAsync<T>(string url)
    {
        HttpResponseMessage response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return default;
    }

    public async Task<AnimeListResponse> GetAnimeListAsync(string userName = "@me", List<AnimeField> fields = null, bool nsfw = true, int offset = 0, int pageSize = 1000)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}users/{userName}/animelist?offset={offset}&limit={pageSize}&{fieldsQuery}&nsfw={nsfw}";

        return await GetAnimeListNextPageAsync(url);
    }

    public async Task<AnimeListResponse> GetAnimeListNextPageAsync(string nextPageUrl)
    {
        return await FetchDataAsync<AnimeListResponse>(nextPageUrl);
    }

    public async Task<AnimeDetails> GetAnimeDetailsAsync(int animeId, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}anime/{animeId}?{fieldsQuery}";

        return await FetchDataAsync<AnimeDetails>(url);
    }

    public async Task<AnimeRankingResponse> GetAnimeRankingAsync(string rankingType = "all", int limit = 10, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}anime/ranking?ranking_type={rankingType}&limit={limit}&{fieldsQuery}";

        return await FetchDataAsync<AnimeRankingResponse>(url);
    }

    public async Task<AnimeSuggestionResponse> GetSuggestedAnimeAsync()
    {
        string url = $"{baseUrl}anime/suggestions";

        return await FetchDataAsync<AnimeSuggestionResponse>(url);
    }

    public async Task<SeasonalAnimeResponse> GetSeasonalAnimeAsync(int year, string season, List<AnimeField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}anime/season/{year}/{season}?{fieldsQuery}";

        return await FetchDataAsync<SeasonalAnimeResponse>(url);
    }

    public async Task<UserInfoResponse> GetMyUserInfoAsync(List<UserInfoField> fields = null)
    {
        string fieldsQuery = fields != null ? $"fields={string.Join(",", fields.Select(f => f.ToApiString()))}" : string.Empty;
        string url = $"{baseUrl}users/@me?{fieldsQuery}";

        return await FetchDataAsync<UserInfoResponse>(url);
    }
}
