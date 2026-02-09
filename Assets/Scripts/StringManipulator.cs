using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using WanaKanaNet;
using WanaKanaNet.Helpers;

public class StringManipulator
{
    private static List<string> SplitString(string input)
    {
        List<string> parts = new();
        string pattern = @"(.*?)\s*\((.*?)\)";
        Regex regex = new(pattern);
        MatchCollection matches = regex.Matches(input);
        bool foundOutsideParentheses = false;

        foreach (Match match in matches)
        {
            string outsideParentheses = match.Groups[1].Value.Trim();
            string insideParentheses = match.Groups[2].Value.Trim();

            if (!string.IsNullOrEmpty(outsideParentheses))
            {
                parts.Add(outsideParentheses);
                foundOutsideParentheses = true;
            }
            if (!string.IsNullOrEmpty(insideParentheses) && !insideParentheses.StartsWith("ep", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(insideParentheses);
            }
        }

        if (!foundOutsideParentheses && matches.Count == 0)
        {
            parts.Add(input.Trim());
        }

        return parts;
    }

    public static SongInfo ExtractSongInfo(string input)
    {
        // Remove initial pattern "#number:" or "#number" and trim
        //string pattern = @"^#\d+:?\s*";
        string regexPattern = @"^#[\da-zA-Z]+:?\s*";
        string cleanedInput = Regex.Replace(input, regexPattern, "");

        // Split the string at the first occurrence of " by " into title and artist
        int byIndex = cleanedInput.IndexOf(" by ", StringComparison.OrdinalIgnoreCase);
        if (byIndex > -1)
        {
            char[] charactersToTrim = { ' ', '\t', '\r', '\n', '\v', '\f', '"' };
            string title = cleanedInput[..byIndex].Trim(charactersToTrim);
            string artist = cleanedInput[(byIndex + 4)..].Trim(charactersToTrim);

            // Return SongInfo object with extracted title and artist
            SongInfo songInfo = new()
            {
                MALSongInfo = new()
                {
                    Titles = SplitString(title),
                    Artists = SplitString(artist)
                },
                SpotifySongInfo = new(),
                Queries = new()
            };

            for (int i = 0; i < songInfo.MALSongInfo.Titles.Count; i++)
            {
                string query = $"{songInfo.MALSongInfo.Titles[i]}";

                if (songInfo.MALSongInfo.Artists.Count > i)
                {
                    query = $"{query} {songInfo.MALSongInfo.Artists[i]}";
                }
                else
                {
                    query = $"{query} {songInfo.MALSongInfo.Artists[0]}";
                }

                songInfo.Queries.Add(query);
            }

            return songInfo;
        }

        // Return null if the string does not match the expected format
        return null;
    }

    // Assume the first name with kana or kanji is the Japanese name, and the first name written in romaji is the English name
    public static Dictionary<string, string> GetLikelyName(List<string> nameList)
    {
        Dictionary<string, string> likelyNames = new()
        {
            ["English"] = null,
            ["Japanese"] = null
        };

        for (int i = 0; i < nameList.Count; i++)
        {
            if (likelyNames["Japanese"] != null && likelyNames["English"] != null)
            {
                break;
            }
            else if (likelyNames["Japanese"] == null && WanaKana.IsJapanese(nameList[i]))
            {
                likelyNames["Japanese"] = nameList[i];
            }
            else if (likelyNames["English"] == null && WanaKana.IsRomaji(nameList[i]))
            {
                likelyNames["English"] = nameList[i];
            }
        }

        Debug.Log($"English: {likelyNames["English"]}, Japanese: {likelyNames["Japanese"]}");

        return likelyNames;
    }

    // public static void GetBestResponse(string malTitle, String malArtist, string spotifyTitle, String spotifyArtist)
    // {
    //     bool titleMatch = false;
    //     bool artistMatch = false;

    //     // Perfect match
    //     if (malTitle == spotifyTitle)
    //     {
    //         titleMatch = true;
    //     }

    //     if (malArtist == spotifyArtist)
    //     {
    //         artistMatch = true;
    //     }

    //     // We have no way to translate kanji, so assume it is a good output
    //     if (WanaKana.IsKanji(spotifyTitle))
    //     {
    //         titleMatch = true;
    //     }

    //     // We have no way to translate kanji, so assume it is a good output
    //     if (WanaKana.IsKanji(spotifyArtist))
    //     {
    //         artistMatch = true;
    //     }
    // }

    public static string ProcessString(string str)
    {
        if (str == null)
        {
            return null;
        }

        string pattern = @"[^a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FAF]";

        return Regex.Replace(str, pattern, "").ToLower();
    }

    public static bool CompareStrings(string str1, string str2)
    {
        if (str1 == null || str2 == null)
        {
            return false;
        }

        return str1.Contains(str2) || str2.Contains(str1);
    }

    public static bool CompareHiragana(string str1, string str2)
    {
        // string str1 = "gotoubunnokatachi";
        // string str2 = "gotobunnokatachi";

        WanaKanaOptions wanaKanaOptions = new()
        {
            CustomKanaMapping = new Dictionary<string, string>
            {
                {"ou", "お"},
                {"kou", "こ"},
                {"sou", "そ"},
                {"tou", "と"},
                {"nou", "の"},
                {"hou", "ほ"},
                {"mou", "も"},
                {"you", "よ"},
                {"rou", "ろ"},
                {"wou", "を"},
                {"gou", "ご"},
                {"zou", "ぞ"},
                {"bou", "ぼ"},
                {"pou", "ぽ"},
                {"kyou", "きょ"},
                {"gyou", "ぎょ"},
                {"shou", "しょ"},
                {"jou", "じょ"},
                {"chou", "ちょ"},
                {"nyou", "にょ"},
                {"hyou", "ひょ"},
                {"byou", "びょ"},
                {"pyou", "ぴょ"},
                {"myou", "みょ"},
                {"ryou", "りょ"}
            }
        };

        string processedStr1 = WanaKana.ToHiragana(str1, wanaKanaOptions);
        string processedStr2 = WanaKana.ToHiragana(str2, wanaKanaOptions);

        return processedStr1 == processedStr2;
    }
}
