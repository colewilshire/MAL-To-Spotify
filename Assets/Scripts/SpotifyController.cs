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

    public Dictionary<Theme, SearchResponse> SearchResponses = new();

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

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = await GetUniqueSongUris();
            //MenuController.Instance.SetMenu(MenuState.Playlist);
            //return;
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

    private async Task<HashSet<string>> GetUniqueSongUris()
    {
        SearchResponses = new();
        HashSet<string> uniqueSongUris = new();
        int iteration = 0;

        foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
        {
            iteration++;
            string query = kvp.Value.SongInfo.Query;

            if (query != null)
            {
                MenuController.Instance.UpdateProgressBar((float)iteration / MALController.Instance.OpeningThemes.Count, query);

                SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
                {
                    Market = "JP",
                    Limit = 1
                };
                SearchResponses.Add(kvp.Value, await spotifyClient.Search.Item(searchRequest));
            }
        }

        foreach (KeyValuePair<Theme, SearchResponse> kvp in SearchResponses)
        {
            // foreach (var a in kvp.Value.Tracks.Items)
            // {
            //     uniqueSongUris.Add(a.Uri);
            // }

            for (int i = 0; i < kvp.Value.Tracks.Items.Count; i++)
            {
                uniqueSongUris.Add(kvp.Value.Tracks.Items[i].Uri);

                kvp.Key.SongInfo.SpotifySongInfo.Add(new()
                {
                    Title = kvp.Value.Tracks.Items[i].Name,
                    Artist = kvp.Value.Tracks.Items[i].Artists[0].Name
                });

                // kvp.Key.SongInfo.SpotifySongInfo[i].Title = kvp.Value.Tracks.Items[i].Name;
                // kvp.Key.SongInfo.SpotifySongInfo[i].Artist = kvp.Value.Tracks.Items[i].Artists[0].Name;

                if (kvp.Value.Tracks.Items[i].LinkedFrom != null)
                {
                    kvp.Key.SongInfo.SpotifySongInfo[i].LinkedId = kvp.Value.Tracks.Items[i].LinkedFrom.Id;
                }

                Debug.Log($"\"{kvp.Key.SongInfo.SpotifySongInfo[i].Title}\", {kvp.Key.SongInfo.SpotifySongInfo[i].Artist} | \"{kvp.Key.SongInfo.Query}\"");
            }

            // if (kvp.Value.Tracks.Items.Count > 0)
            // {
            //     uniqueSongUris.Add(kvp.Value.Tracks.Items[0].Uri);

            //     kvp.Key.SongInfo.SpotifySongInfo.Title = kvp.Value.Tracks.Items[0].Name;
            //     kvp.Key.SongInfo.SpotifySongInfo.Artist = kvp.Value.Tracks.Items[0].Artists[0].Name;

            //     if (kvp.Value.Tracks.Items[0].LinkedFrom != null)
            //     {
            //         kvp.Key.SongInfo.SpotifySongInfo.LinkedId = kvp.Value.Tracks.Items[0].LinkedFrom.Id;
            //     }
            // }
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
