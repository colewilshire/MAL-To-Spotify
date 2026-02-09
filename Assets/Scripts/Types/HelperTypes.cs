using System.Collections.Generic;

public class SongInfo
{
    public MALSongInfo MALSongInfo { get; set; }
    public List<SpotifySongInfo> SpotifySongInfo { get; set; }
    public List<string> Queries { get; set; }
}

public class MALSongInfo
{
    public List<string> Titles { get; set; }
    public List<string> Artists { get; set; }
}

public class SpotifySongInfo
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string LinkedId { get; set; }
}