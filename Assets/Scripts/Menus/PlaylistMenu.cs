using System.Collections.Generic;
using SpotifyAPI.Web;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistMenu : Menu
{
    protected override MenuState ActiveState { get; set; } = MenuState.Playlist;
    [SerializeField] private GameObject scrollViewContent;
    [SerializeField] private SongEntry songEntryTemplate;
    [SerializeField] private Button confirmButton;

    protected override void Start()
    {
        base.Start();

        confirmButton.onClick.AddListener(() => ApprovePlaylist());
    }

    protected override void OnMenuStateChange(MenuState newState)
    {
        base.OnMenuStateChange(newState);
        if (newState != ActiveState) { return; }
        DisplayPlaylist();
    }

    public void DisplayPlaylist()//Dictionary<Theme, SearchResponse> searchResponses, int recursion = 0)
    {
        // if (recursion > 0)
        // {
        //     SpotifyController.Instance.SearchResponses = new();
        // }

        if (SpotifyController.Instance.SearchResponses == null) { return; }

        foreach (KeyValuePair<Theme, SearchResponse> kvp in SpotifyController.Instance.SearchResponses)
        {
            SongEntry songEntry = Instantiate(songEntryTemplate);
            string title = kvp.Value.Tracks.Items[0].Name;
            string artist = kvp.Value.Tracks.Items[0].Artists[0].Name;

            songEntry.name = title;
            songEntry.Title.text = title;
            songEntry.Artist.text = artist;

            songEntry.transform.SetParent(songEntryTemplate.transform.parent);
            songEntry.gameObject.SetActive(true);
        }
    }

    public void ApprovePlaylist()
    {

    }
}
