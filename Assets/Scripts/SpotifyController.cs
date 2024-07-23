using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpotifyController : Singleton<SpotifyController>
{
    [SerializeField] private TMP_InputField spotifyInputField;
    [SerializeField] private Button spotifyLoginButton;
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
        spotifyClient = await AuthenticationController.Instance.AuthenticateSpotifyClient();

        PrivateUser currentUser = await spotifyClient.UserProfile.Current();
        spotifyInputField.text = currentUser.Id;

        PlaylistCreateRequest playlistCreateRequest = new(playlistName);
        FullPlaylist playlist = await spotifyClient.Playlists.Create(currentUser.Id, playlistCreateRequest);

        if (MALController.Instance.OpeningThemes != null)
        {
            HashSet<string> uniqueSongUris = new();

            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                Theme themeSong = kvp.Value;

                if (themeSong != null && themeSong.Text != null)
                {
                    string songName = FormatSongName(themeSong.Text);

                    if (songName != null)
                    {
                        spotifyInputField.text = songName;

                        SearchRequest searchRequest = new(SearchRequest.Types.Track, songName);
                        SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

                        if (searchResponse.Tracks.Items.Count > 0)
                        {
                            uniqueSongUris.Add(searchResponse.Tracks.Items[0].Uri);
                        }
                    }
                }
            }

            List<List<string>> pagedSongUris = SplitIntoBatches(uniqueSongUris, 100);

            foreach (List<string> songUriPage in pagedSongUris)
            {
                PlaylistAddItemsRequest playlistAddItemsRequest = new(songUriPage);
                await spotifyClient.Playlists.AddItems(playlist.Id, playlistAddItemsRequest);
            }

            spotifyInputField.text = "Done";
        }
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

    public static string FormatSongName(string input)
    {
        // Define a regex pattern to capture the quoted substring and the substring after "by "
        string pattern = "\"([^\"]*)\".*? by (.*)";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            return $"{match.Groups[1].Value} {match.Groups[2].Value}";
        }
        
        return null;
    }
}
