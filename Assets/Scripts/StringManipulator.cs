using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WanaKanaNet;

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
        string pattern = @"^#\d+:?\s*";
        string cleanedInput = Regex.Replace(input, pattern, "").Trim();

        // Split the string at the first occurrence of " by " into title and artist
        int byIndex = cleanedInput.IndexOf(" by ", StringComparison.OrdinalIgnoreCase);
        if (byIndex > -1)
        {
            string title = cleanedInput[..byIndex].Trim();
            string artist = cleanedInput[(byIndex + 4)..].Trim();

            // Return SongInfo object with extracted title and artist
            SongInfo songInfo = new()
            {
                MALSongInfo = new()
                {
                    Titles = SplitString(title),
                    Artists = SplitString(artist)
                },
                SpotifySongInfo = new()
            };

            songInfo.SpotifySongInfo.Query = $"{songInfo.MALSongInfo.Titles[0]} {songInfo.MALSongInfo.Artists[0]}";

            return songInfo;
        }

        // Return null if the string does not match the expected format
        return null;
    }

    public static string ProcessString(string str)
    {
        //
        if (str == null)
        {
            return null;
        }
        //

        string pattern = @"[^a-zA-Z0-9\u3040-\u30FF\u4E00-\u9FAF]";

        return Regex.Replace(str, pattern, "").ToLower();
    }

    public static bool CompareStrings(string str1, string str2)
    {
        //
        if (str1 == null || str2 == null)
        {
            return false;
        }
        //

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
