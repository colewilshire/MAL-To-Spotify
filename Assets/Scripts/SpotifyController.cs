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

        SearchRequest searchRequest = new(SearchRequest.Types.Track, "アイドル");
        SearchResponse searchResponse = await spotifyClient.Search.Item(searchRequest);
        string trackUri = searchResponse.Tracks.Items[0].Uri;
        spotifyInputField.text = trackUri;

        List<string> songUris = new() {trackUri};
        PlaylistAddItemsRequest playlistAddItemsRequest = new(songUris);

        await spotifyClient.Playlists.AddItems(playlist.Id, playlistAddItemsRequest);
    }
}
