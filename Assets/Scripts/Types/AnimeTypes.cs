using System.Collections.Generic;

public class AnimeDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public MainPicture MainPicture { get; set; }
    public AlternativeTitles AlternativeTitles { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Synopsis { get; set; }
    public double Mean { get; set; }
    public int Rank { get; set; }
    public int Popularity { get; set; }
    public int NumListUsers { get; set; }
    public int NumScoringUsers { get; set; }
    public string Nsfw { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public string MediaType { get; set; }
    public string Status { get; set; }
    public List<Genre> Genres { get; set; }
    public MyListStatus MyListStatus { get; set; }
    public int NumEpisodes { get; set; }
    public StartSeason StartSeason { get; set; }
    public Broadcast Broadcast { get; set; }
    public string Source { get; set; }
    public int AverageEpisodeDuration { get; set; }
    public string Rating { get; set; }
    public List<Picture> Pictures { get; set; }
    public string Background { get; set; }
    public List<RelatedAnime> RelatedAnime { get; set; }
    public List<Recommendation> Recommendations { get; set; }
    public List<Studio> Studios { get; set; }
    public Statistics Statistics { get; set; }
    public List<Theme> OpeningThemes { get; set; }
    public List<Theme> EndingThemes { get; set; }
}

public class MainPicture
{
    public string Medium { get; set; }
    public string Large { get; set; }
}

public class AlternativeTitles
{
    public List<string> Synonyms { get; set; }
    public string En { get; set; }
    public string Ja { get; set; }
}

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class MyListStatus
{
    public string Status { get; set; }
    public int Score { get; set; }
    public int NumEpisodesWatched { get; set; }
    public bool IsRewatching { get; set; }
    public string UpdatedAt { get; set; }
    public string StartDate { get; set; }
}

public class StartSeason
{
    public int Year { get; set; }
    public string Season { get; set; }
}

public class Broadcast
{
    public string DayOfTheWeek { get; set; }
    public string StartTime { get; set; }
}

public class Picture
{
    public string Medium { get; set; }
    public string Large { get; set; }
}

public class RelatedAnime
{
    public Node Node { get; set; }
    public string RelationType { get; set; }
    public string RelationTypeFormatted { get; set; }
}

public class Node
{
    public int Id { get; set; }
    public string Title { get; set; }
    public MainPicture MainPicture { get; set; }
}

public class Recommendation
{
    public Node Node { get; set; }
    public int NumRecommendations { get; set; }
}

public class Studio
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Statistics
{
    public Status Status { get; set; }
    public int NumListUsers { get; set; }
}

public class Status
{
    public string Watching { get; set; }
    public string Completed { get; set; }
    public string OnHold { get; set; }
    public string Dropped { get; set; }
    public string PlanToWatch { get; set; }
}

public class Theme
{
    public int Id { get; set; }
    public int AnimeId { get; set; }
    public string Text { get; set; }
    public SongInfo SongInfo { get; set; }
}

public class SongInfo
{
    public MALSongInfo MALSongInfo { get; set; }
    public SpotifySongInfo SpotifySongInfo { get; set; }
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
    public string Query { get; set; }
}

public class AnimeListResponse
{
    public List<AnimeListNode> Data { get; set; }
    public Paging Paging { get; set; }
}

public class AnimeListNode
{
    public AnimeNode Node { get; set; }
}

public class AnimeNode
{
    public int Id { get; set; }
    public string Title { get; set; }
    public MainPicture MainPicture { get; set; }
}

public class Paging
{
    public string Previous { get; set; }
    public string Next { get; set; }
}

public class AnimeRankingResponse
{
    public List<RankingNode> Data { get; set; }
}

public class RankingNode
{
    public AnimeNode Node { get; set; }
    public Ranking Ranking { get; set; }
}

public class Ranking
{
    public int Rank { get; set; }
}

public class AnimeSuggestionResponse
{
    public List<AnimeListNode> Data { get; set; }
}

public class SeasonalAnimeResponse
{
    public List<AnimeListNode> Data { get; set; }
    public Season Season { get; set; }
    public Paging Paging { get; set; }
}

public class Season
{
    public int Year { get; set; }
    public string SeasonName { get; set; }
}
