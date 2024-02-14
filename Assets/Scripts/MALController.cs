using System;
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

    public async Task GetAnimeDetailsAsync(int animeId)
    {
        string fields = "opening_themes,ending_themes";
        string url = $"{baseUrl}anime/{animeId}?fields={fields}";
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", malAuthenticator.TokenResponse.access_token);

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                Debug.Log(jsonString);
                //AnimeDetails animeDetails = JsonSerializer.Deserialize<AnimeDetails>(jsonString);

                // if (animeDetails != null)
                // {
                //     Debug.Log($"ID: {animeDetails.Id}, Title: {animeDetails.Title}");
                //     animeDetailsText.text = $"ID: {animeDetails.Id}, Title: {animeDetails.Title}";
                // }
            }
            else
            {
                Debug.LogError($"Failed to fetch anime details: {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception occurred while fetching anime details: {e.Message}");
        }
    }

    private void Start()
    {
        malAuthenticator = GetComponent<MALAuthenticator>();

        //button2.onClick.AddListener(async () => await GetUsersAnimeListAsync());
        button2.onClick.AddListener(async () => await GetAnimeDetailsAsync(52034));
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
                anime.text = $"ID: {animeData.Node.Id}, Title: {animeData.Node.Title}";
            }
        }
    }
}
