using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.Json;

public class MALController : MonoBehaviour
{
    private readonly string baseUrl = "https://api.myanimelist.net/v2/";
    private MALAuthenticator malAuthenticator;

    //
    [SerializeField] private Button button2;
    //

    private void Start()
    {
        malAuthenticator = GetComponent<MALAuthenticator>();

        button2.onClick.AddListener(() => StartCoroutine(GetUsersAnimeList()));
    }

    private IEnumerator GetUsersAnimeList(string userName = "@me")
    {
        string url = $"{baseUrl}users/{userName}/animelist";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {malAuthenticator.TokenResponse.access_token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(request.downloadHandler.text);
            DeserializeAnimeList(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Failed to fetch data: {request.error}");
        }
    }

    private void DeserializeAnimeList(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Helps with case sensitivity
        };

        AnimeListResponse animeListResponse = JsonSerializer.Deserialize<AnimeListResponse>(jsonString, options);

        // Example usage
        if (animeListResponse != null && animeListResponse.Data != null)
        {
            foreach (var animeData in animeListResponse.Data)
            {
                Debug.Log($"ID: {animeData.Node.Id}, Title: {animeData.Node.Title}");
            }
        }
    }
}
