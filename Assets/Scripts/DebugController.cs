using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using WanaKanaNet;
using WanaKanaNet.Helpers;
using UnityEngine;

public class DebugController : Singleton<DebugController>
{
    private bool VerifyTitle(List<string> malTitles, string spotifyTitle)
    {
        bool titleFound = false;

        foreach (string title in malTitles)
        {
            if (title != null && spotifyTitle != null)  //
            {
                string processedMALTitle = StringManipulator.ProcessString(title);
                string processedSpotifyTitle = StringManipulator.ProcessString(spotifyTitle);

                if (StringManipulator.CompareStrings(processedMALTitle, processedSpotifyTitle))
                {
                    titleFound = true;
                }
                else if (StringManipulator.CompareHiragana(processedMALTitle, processedSpotifyTitle))
                {
                    //Debug.Log(title);
                    titleFound = true;
                }
            }
        }

        return titleFound;
    }

    private bool VerifyArtist(List<string> malArtists, string spotifyArtist)
    {
        bool artistFound = false;

        foreach (string artist in malArtists)
        {
            if (artist != null && spotifyArtist != null)  //
            {
                string processedMALArtist = StringManipulator.ProcessString(artist);
                string processedSpotifyArtist = StringManipulator.ProcessString(spotifyArtist);

                if (WanaKana.IsKanji(processedSpotifyArtist))
                {
                    artistFound = true;
                }
                else if (StringManipulator.CompareStrings(processedMALArtist, processedSpotifyArtist))
                {
                    artistFound = true;
                }
                else if (StringManipulator.CompareHiragana(processedMALArtist, processedSpotifyArtist))
                {
                    //Debug.Log(artist);
                    artistFound = true;
                }
                else
                {
                    Token[] tokenizedArtist = WanaKana.Tokenize(processedSpotifyArtist);
                    
                    foreach(Token token in tokenizedArtist)
                    {
                        if (WanaKana.IsKanji(token.Content))
                        {
                            artistFound = true;
                            break;
                        }
                    }
                }
            }
        }

        return artistFound;
    }

    public void ExportSongList(Dictionary<int, Theme> themeSongList, string saveName, string fileExtension)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}.{fileExtension}");
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string serializedList = JsonSerializer.Serialize(themeSongList, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
        NativeFilePicker.ExportFile(savedListPath);
    }

    public void GetStats(Dictionary<int, Theme> openingThemes)
    {
        int titleMismatches = 0;
        int artistMismatches = 0;
        Dictionary<int, Theme> verifiedSongs = new();
        Dictionary<int, Theme> rejectedTitles = new();
        Dictionary<int, Theme> rejectedArtists = new();
        Dictionary<int, Theme> completeRejections = new();

        foreach (KeyValuePair<int, Theme> kvp in openingThemes)
        {
            bool titleFound = false;
            bool artistFound = false;
            
            titleFound = VerifyTitle(kvp.Value.SongInfo.MALSongInfo.Titles, kvp.Value.SongInfo.SpotifySongInfo.Title);

            if (titleFound == false)
            {
                titleMismatches++;

                string processedStr1 = StringManipulator.ProcessString(kvp.Value.SongInfo.MALSongInfo.Titles[0]);
                string processedStr2 = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Title);
                //Debug.Log($"{processedStr1} : {processedStr2}");
            }

            artistFound = VerifyArtist(kvp.Value.SongInfo.MALSongInfo.Artists, kvp.Value.SongInfo.SpotifySongInfo.Artist);

            if (artistFound == false)
            {
                artistMismatches++;

                string processedStr1 = StringManipulator.ProcessString(kvp.Value.SongInfo.MALSongInfo.Artists[0]);
                string processedStr2 = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Artist);
                //Debug.Log($"{processedStr1} : {processedStr2}");
            }

            if (titleFound && artistFound)
            {
                verifiedSongs.Add(kvp.Key, kvp.Value);
            }
            else if(titleFound == false && artistFound == true)
            {
                rejectedTitles.Add(kvp.Key, kvp.Value);
            }
            else if(titleFound == true && artistFound == false)
            {
                rejectedArtists.Add(kvp.Key, kvp.Value);
            }
            else
            {
                completeRejections.Add(kvp.Key, kvp.Value);
            }
        }

        ExportSongList(verifiedSongs, "VerifiedSongs", "txt");
        ExportSongList(rejectedTitles, "RejectedTitles", "txt");
        ExportSongList(rejectedArtists, "RejectedArtists", "txt");
        ExportSongList(completeRejections, "CompleteRejections", "txt");

        float a = (float) titleMismatches/openingThemes.Count*100;
        float b = (float) artistMismatches/openingThemes.Count*100;

        Debug.Log($"Title: {titleMismatches}: {a}%"); // 67.65% // 61.41%      //51.97% //31.3% //30.71%    //29.72%    //28.74
        Debug.Log($"Artist: {artistMismatches}: {b}%"); // 55.22%   // 56.89%      //56.89 //46.26%                     //39.37 //33.07 //32.87 //28.14 //27.36
    }
}
