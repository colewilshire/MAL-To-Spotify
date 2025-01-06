using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using System;

public class SpotifyController : Singleton<SpotifyController>
{
    [SerializeField] private TMP_InputField spotifyInputField;
    [SerializeField] private Button spotifyLoginButton;
    [SerializeField] private string playlistName = "Anime Opening Themes";

    private SpotifyClient spotifyClient;

    private void Start()
    {
        spotifyLoginButton.onClick.AddListener(async () => await Test());
    }

    private async Task Test()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);
        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        MenuController.Instance.UpdateProgressBar(0, currentUser.Id);
        spotifyInputField.text = currentUser.Id;

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = new();

            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                string query = kvp.Value.SongInfo.SpotifySongInfo.Query;

                if (query != null)
                {
                    MenuController.Instance.UpdateProgressBar(0, query);

                    SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
                    {
                        Market = "JP",
                        Limit = 1
                    };
                    SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

                    if (searchResponse.Tracks.Items.Count > 0)
                    {
                        uniqueSongUris.Add(searchResponse.Tracks.Items[0].Uri);

                        kvp.Value.SongInfo.SpotifySongInfo.Title = searchResponse.Tracks.Items[0].Name;
                        kvp.Value.SongInfo.SpotifySongInfo.Artist = searchResponse.Tracks.Items[0].Artists[0].Name;

                        if (searchResponse.Tracks.Items[0].LinkedFrom != null)
                        {
                            kvp.Value.SongInfo.SpotifySongInfo.LinkedId = searchResponse.Tracks.Items[0].LinkedFrom.Id;
                        }
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

            MenuController.Instance.UpdateProgressBar(0, "Done");
        }

        MenuController.Instance.SetMenu(MenuState.Main);
    }

    private async Task Test2()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);
        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        MenuController.Instance.UpdateProgressBar(0, currentUser.Id);
        spotifyInputField.text = currentUser.Id;

        //await Test1();
        //MenuController.Instance.SetMenu(MenuState.Main);
        //return;

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = new();

            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                string query = kvp.Value.SongInfo.SpotifySongInfo.Query;

                if (query != null)
                {
                    MenuController.Instance.UpdateProgressBar(0, query);

                    SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
                    {
                        Market = "JP",
                        Limit = 1
                    };
                    SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

                    if (searchResponse.Tracks.Items.Count > 0)
                    {
                        uniqueSongUris.Add(searchResponse.Tracks.Items[0].Uri);

                        kvp.Value.SongInfo.SpotifySongInfo.Title = searchResponse.Tracks.Items[0].Name;
                        kvp.Value.SongInfo.SpotifySongInfo.Artist = searchResponse.Tracks.Items[0].Artists[0].Name;

                        if (searchResponse.Tracks.Items[0].LinkedFrom != null)
                        {
                            kvp.Value.SongInfo.SpotifySongInfo.LinkedId = searchResponse.Tracks.Items[0].LinkedFrom.Id;
                        }
                    }
                }
            }

            ////
            Dictionary<int, KeyValuePair<List<string>, List<string>>> q = new();
            Dictionary<int, KeyValuePair<string, List<string>>> r = new();
            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                q.Add(kvp.Key, new(kvp.Value.SongInfo.MALSongInfo.Titles, kvp.Value.SongInfo.MALSongInfo.Artists));
                r.Add(kvp.Key, new(kvp.Value.SongInfo.SpotifySongInfo.Title, kvp.Value.SongInfo.MALSongInfo.Artists));
            }
            string x = JsonConvert.SerializeObject(q);
            string y = JsonConvert.SerializeObject(r);
            Debug.Log(x);
            Debug.Log(y);
            return;
            ////

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

    private async Task Test3()
    {
        MenuController.Instance.SetMenu(MenuState.Loading);
        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        MenuController.Instance.UpdateProgressBar(0, currentUser.Id);
        spotifyInputField.text = currentUser.Id;

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = await GetUniqueSongUris();
            Debug.Log("1111");
            List<List<string>> pagedSongUris = SplitIntoBatches(uniqueSongUris, 100);
            PlaylistCreateRequest playlistCreateRequest = new(playlistName);
            FullPlaylist playlist = await spotifyClient.Playlists.Create(currentUser.Id, playlistCreateRequest);
            Debug.Log("2222");

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
        Dictionary<Theme, Task<SearchResponse>> searchResponses = new();
        HashSet<string> uniqueSongUris = new();

        foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
        {
            string query = kvp.Value.SongInfo.SpotifySongInfo.Query;

            if (query != null)
            {
                MenuController.Instance.UpdateProgressBar(0, query);

                SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
                {
                    Market = "JP",
                    Limit = 1
                };
                searchResponses.Add(kvp.Value, spotifyClient.Search.Item(searchRequest));
            }
        }

        try
        {
            await Task.WhenAll(searchResponses.Values);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        foreach (KeyValuePair<Theme, Task<SearchResponse>> kvp in searchResponses)
        {
            Debug.Log("tttt");
            if (kvp.Value.Result.Tracks.Items.Count > 0)
            {
                Debug.Log("qqqqq");
                uniqueSongUris.Add(kvp.Value.Result.Tracks.Items[0].Uri);

                kvp.Key.SongInfo.SpotifySongInfo.Title = kvp.Value.Result.Tracks.Items[0].Name;
                kvp.Key.SongInfo.SpotifySongInfo.Artist = kvp.Value.Result.Tracks.Items[0].Artists[0].Name;

                if (kvp.Value.Result.Tracks.Items[0].LinkedFrom != null)
                {
                    kvp.Key.SongInfo.SpotifySongInfo.LinkedId = kvp.Value.Result.Tracks.Items[0].LinkedFrom.Id;
                }
                Debug.Log("zzzz");
            }
        }

        return uniqueSongUris;
    }

    // private async Task<SearchResponse> SafeSearchRequest(SearchRequest searchRequest)
    // {
    //     try
    //     {
    //         return await spotifyClient.Search.Item(searchRequest);
    //     }
    //     catch
    //     {
    //         return null;
    //     }
    // }

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

    private async Task Test1()
    {
        List<string> queryList = new()
        {
            "Yume Tourou 夢灯籠 RADWIMPS",
            "Dramatic ja Nakute mo ドラマチックじゃなくても Kana Hanazawa"
        };

        foreach (string query in queryList)
        {
            SearchRequest searchRequest = new(SearchRequest.Types.Track, query)
            {
                Market = "US",
                Limit = 5
            };
            SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

            for (int i = 0; i < searchResponse.Tracks.Items.Count; i++)
            {
                FullTrack track = searchResponse.Tracks.Items[i];

                if (track.LinkedFrom != null)
                {
                    FullTrack alternateTrack = await spotifyClient.Tracks.Get(track.LinkedFrom.Id);
                    Debug.Log($"{i}: {track.Name}, {track.Artists[0].Name} : {alternateTrack.Name}, {alternateTrack.Artists[0].Name}");
                }
                else
                {
                    Debug.Log($"{i}: {track.Name}, {track.Artists[0].Name} : null");
                }
            }

            // TracksRequest tracksRequest = new(new List<string>());
            // TracksResponse tracksResponse = await spotifyClient.Tracks.GetSeveral(tracksRequest);
            // foreach(FullTrack fullTrack in tracksResponse.Tracks)
            // {

            // }
        }
    }
}
