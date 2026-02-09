using System.Collections.Generic;

public class APIBridge : Singleton<APIBridge>
{
    private List<List<string>> spotifyQueries;

    public void PopulateMALInfo(Dictionary<int, Theme> openingThemes)
    {
        spotifyQueries = new();

        foreach (KeyValuePair<int, Theme> kvp in openingThemes)
        {
            SongInfo songInfo = StringManipulator.ExtractSongInfo(kvp.Value.Text);
            spotifyQueries.Add(songInfo.Queries);
        }
    }

    public List<List<string>> GetQueries()
    {
        if (spotifyQueries != null)
        {
            return spotifyQueries;
        }

        return null;
    }
}
