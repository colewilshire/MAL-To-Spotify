using System;
using System.Linq;
using System.Collections.Generic;

public enum UserInfoField
{
    Id,
    Name,
    Picture,
    Gender,
    Birthday,
    Location,
    JoinedAt,
    AnimeStatistics,
    TimeZone,
    IsSupporter
}

public static class UserInfoFieldExtensions
{
    public static string ToApiString(this UserInfoField field)
    {
        return field switch
        {
            UserInfoField.JoinedAt => "joined_at",
            UserInfoField.AnimeStatistics => "anime_statistics",
            UserInfoField.TimeZone => "time_zone",
            UserInfoField.IsSupporter => "is_supporter",

            // Default conversion for any fields that directly match their enum names in snake_case
            _ => string.Concat(field.ToString().Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString().ToLower() : x.ToString().ToLower()))
        };
    }

    // Method to get all fields
    public static List<UserInfoField> GetAllFields()
    {
        return Enum.GetValues(typeof(UserInfoField)).Cast<UserInfoField>().ToList();
    }
}
