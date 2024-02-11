using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MALController : MonoBehaviour
{
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";
    private MALAuthenticator malAuthenticator;
    private readonly HttpClient httpClient = new();

    [SerializeField] private Button button2;

    private void Start()
    {
        malAuthenticator = GetComponent<MALAuthenticator>();

        button2.onClick.AddListener(async () => await GetUsersAnimeListAsync());
    }

    private async Task GetUsersAnimeListAsync(string userName = "@me")
    {
        string url = $"{baseUrl}users/{userName}/animelist";
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", malAuthenticator.TokenResponse.access_token);

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                DeserializeAnimeList(jsonString);
            }
            else
            {
                Debug.LogError($"Failed to fetch data: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception occurred: {e.Message}");
        }
    }

    private void DeserializeAnimeList(string jsonString)
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        AnimeListResponse animeListResponse = JsonSerializer.Deserialize<AnimeListResponse>(jsonString, options);

        if (animeListResponse != null && animeListResponse.Data != null)
        {
            foreach (AnimeData animeData in animeListResponse.Data)
            {
                Debug.Log($"ID: {animeData.Node.Id}, Title: {animeData.Node.Title}");
            }
        }
    }
}
