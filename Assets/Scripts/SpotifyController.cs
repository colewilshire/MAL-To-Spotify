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

        PlaylistCreateRequest playlistCreateRequest = new("Anime Opening Themes");
        FullPlaylist playlist = await spotifyClient.Playlists.Create(currentUser.Id, playlistCreateRequest);

        if (MALController.Instance.OpeningThemes != null)
        {
            List<List<string>> pagedSongUris = new();
            int batchSize = 100;
            int index = 0;

            foreach (KeyValuePair<int, Theme> kvp in MALController.Instance.OpeningThemes)
            {
                Theme themeSong = kvp.Value;

                // Spotify limits requests to a batch size of 100
                if (index % batchSize == 0)
                {
                    pagedSongUris.Add(new List<string>());
                }

                if (themeSong != null && themeSong.Text != null)
                {
                    string songName = FormatSongName(themeSong.Text);

                    if (songName != null)
                    {
                        spotifyInputField.text = $"{index}: {songName}";

                        SearchRequest searchRequest = new(SearchRequest.Types.Track, songName);
                        SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);

                        if (searchResponse.Tracks.Items.Count > 0)
                        {
                            pagedSongUris[index / batchSize].Add(searchResponse.Tracks.Items[0].Uri);
                        }
                    }
                }

                index++;
            }

            foreach (List<string> songUriPage in pagedSongUris)
            {
                PlaylistAddItemsRequest playlistAddItemsRequest = new(songUriPage);
                await spotifyClient.Playlists.AddItems(playlist.Id, playlistAddItemsRequest);
            }

            spotifyInputField.text = "Done";
        }
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
        else
        {
            return null;
        }
    }
}
