using System;
using System.Linq;
using System.Collections.Generic;

public enum AnimeField
{
    Id,
    Title,
    MainPicture,
    AlternativeTitles,
    StartDate,
    EndDate,
    Synopsis,
    Mean,
    Rank,
    Popularity,
    NumListUsers,
    NumScoringUsers,
    Nsfw,
    CreatedAt,
    UpdatedAt,
    MediaType,
    Status,
    Genres,
    MyListStatus,
    NumEpisodes,
    StartSeason,
    Broadcast,
    Source,
    AverageEpisodeDuration,
    Rating,
    Pictures,
    Background,
    RelatedAnime,
    RelatedManga,
    Recommendations,
    Studios,
    Statistics,
    OpeningThemes,
    EndingThemes
}

public static class AnimeFieldExtensions
{
    public static string ToApiString(this AnimeField field)
    {
        return field switch
        {
            AnimeField.MainPicture => "main_picture",
            AnimeField.AlternativeTitles => "alternative_titles",
            AnimeField.NumListUsers => "num_list_users",
            AnimeField.NumScoringUsers => "num_scoring_users",
            AnimeField.MyListStatus => "my_list_status",
            AnimeField.NumEpisodes => "num_episodes",
            AnimeField.StartSeason => "start_season",
            AnimeField.AverageEpisodeDuration => "average_episode_duration",
            AnimeField.RelatedAnime => "related_anime",
            AnimeField.RelatedManga => "related_manga",
            AnimeField.OpeningThemes => "opening_themes",
            AnimeField.EndingThemes => "ending_themes",

            // Default conversion for any fields that directly match their enum names in snake_case
            _ => string.Concat(field.ToString().Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString().ToLower() : x.ToString().ToLower()))
        };
    }

    // Method to get all fields
    public static List<AnimeField> GetAllFields()
    {
        return Enum.GetValues(typeof(AnimeField)).Cast<AnimeField>().ToList();
    }
}
