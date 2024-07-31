using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpotifyController : Singleton<SpotifyController>
{
    [SerializeField] private TMP_InputField spotifyInputField;
    [SerializeField] private Button spotifyLoginButton;
    [SerializeField] private Button malLoginButton;
    [SerializeField] private string clientId;
    [SerializeField] private string clientSecret;
    [SerializeField] private string playlistName = "Anime Opening Themes";

    private SpotifyClient spotifyClient;

    private void Start()
    {
        spotifyLoginButton.onClick.AddListener(async () => await Test());
    }

    private async Task Test()
    {
        spotifyLoginButton.interactable = false;
        malLoginButton.interactable = false;

        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        spotifyInputField.text = currentUser.Id;

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = new();

            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                string query = kvp.Value.SongInfo.SpotifySongInfo.Query;

                if (query != null)
                {
                    spotifyInputField.text = query;

                    SearchRequest searchRequest = new(SearchRequest.Types.Track, query);
                    SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

                    if (searchResponse.Tracks.Items.Count > 0)
                    {
                        uniqueSongUris.Add(searchResponse.Tracks.Items[0].Uri);

                        kvp.Value.SongInfo.SpotifySongInfo.Title = searchResponse.Tracks.Items[0].Name;
                        kvp.Value.SongInfo.SpotifySongInfo.Artist = searchResponse.Tracks.Items[0].Artists[0].Name;
                    }
                }
            }

            List<List<string>> pagedSongUris = SplitIntoBatches(uniqueSongUris, 100);
            PlaylistCreateRequest playlistCreateRequest = new(playlistName);
            FullPlaylist playlist = await spotifyClient.Playlists.Create(currentUser.Id, playlistCreateRequest);

            foreach (List<string> songUriPage in pagedSongUris)
            {
                PlaylistAddItemsRequest playlistAddItemsRequest = new(songUriPage);
                await spotifyClient.Playlists.AddItems(playlist.Id, playlistAddItemsRequest);
            }

            spotifyInputField.text = "Done";
        }

        spotifyLoginButton.interactable = true;
        malLoginButton.interactable = true;
    }

    private List<List<string>> SplitIntoBatches(HashSet<string> uniqueSongUris, int batchSize)
    {
        List<List<string>> pagedSongUris = new();
        List<string> currentBatch = new();

        foreach (string uri in uniqueSongUris)
        {
            if (currentBatch.Count == batchSize)
            {
                pagedSongUris.Add(currentBatch);
                currentBatch = new List<string>();
            }
            currentBatch.Add(uri);
        }

        if (currentBatch.Count > 0)
        {
            pagedSongUris.Add(currentBatch);
        }

        return pagedSongUris;
    }
}
