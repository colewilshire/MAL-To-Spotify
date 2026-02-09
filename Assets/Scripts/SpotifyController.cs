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
    [SerializeField] private string playlistName = "Anime Opening Themes";

    public List<SearchResponse> SearchResponses = new();

    private SpotifyClient spotifyClient;

    private void Start()
    {
        spotifyLoginButton.onClick.AddListener(async () => await Test4());
    }

    private async Task Test4()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);
        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        MenuController.Instance.UpdateProgressBar(0, currentUser.Id);
        spotifyInputField.text = currentUser.Id;

        List<List<string>> queries = APIBridge.Instance.GetQueries();
        if (queries != null)
        {
            HashSet<string> uniqueSongUris = await GetUniqueSongUris(queries);
            List<List<string>> pagedSongUris = SplitIntoBatches(uniqueSongUris, 100);
            PlaylistCreateRequest playlistCreateRequest = new(playlistName);
            FullPlaylist playlist = await spotifyClient.Playlists.Create(currentUser.Id, playlistCreateRequest);

            foreach (List<string> songUriPage in pagedSongUris)
            {
                PlaylistAddItemsRequest playlistAddItemsRequest = new(songUriPage);
                await spotifyClient.Playlists.AddItems(playlist.Id, playlistAddItemsRequest);
            }

            MenuController.Instance.UpdateProgressBar(0, "Done");
        }

        MenuController.Instance.SetMenu(MenuState.Main);
    }

    private async Task<HashSet<string>> GetUniqueSongUris(List<List<string>> queries)
    {
        SearchResponses = new();
        HashSet<string> uniqueSongUris = new();
        int iteration = 0;

        foreach (List<string> queryPair in queries)
        {
            iteration++;

            foreach (string query in queryPair)
            {
                MenuController.Instance.UpdateProgressBar((float)iteration / queries.Count, query);

                SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
                {
                    Market = "JP",
                    Limit = 1
                };
                SearchResponses.Add(await spotifyClient.Search.Item(searchRequest));
            }
        }

        foreach (SearchResponse searchResponse in SearchResponses)
        {
            if (searchResponse.Tracks.Items.Count > 0)
            {
                uniqueSongUris.Add(searchResponse.Tracks.Items[0].Uri);  // Save only the first query result to include in the playlist
            }
        }

        return uniqueSongUris;
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

    public async Task<string> GetAlternateTitle(string linkedId)
    {
        FullTrack linkedTrack = await spotifyClient.Tracks.Get(linkedId);
        return linkedTrack.Name;
    }
}
