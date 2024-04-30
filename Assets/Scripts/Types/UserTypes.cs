public class UserInfoResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Picture { get; set; }
    public string Gender { get; set; }
    public string Birthday { get; set; }
    public string Location { get; set; }
    public string JoinedAt { get; set; }
    public AnimeStatistics AnimeStatistics { get; set; }
    public string TimeZone { get; set; }
    public bool? IsSupporter { get; set; }
}

public class AnimeStatistics
{
    public int NumItemsWatching { get; set; }
    public int NumItemsCompleted { get; set; }
    public int NumItemsOnHold { get; set; }
    public int NumItemsDropped { get; set; }
    public int NumItemsPlanToWatch { get; set; }
}