using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using WanaKanaNet;
using WanaKanaNet.Helpers;
using UnityEngine;

public class DebugController : Singleton<DebugController>
{
    private async Task<bool> VerifyTitle(MALSongInfo malSongInfo, SpotifySongInfo spotifySongInfo)
    {
        bool titleFound = false;
        List<string> malTitles = malSongInfo.Titles;
        string spotifyTitle = spotifySongInfo.Title;

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
                    titleFound = true;
                }
                else if (spotifySongInfo.LinkedId != null)
                {
                    string alternateTitle = await SpotifyController.Instance.GetAlternateTitle(spotifySongInfo.LinkedId);

                    if (alternateTitle != null)
                    {
                        string processedAlternateTitle = StringManipulator.ProcessString(alternateTitle);

                        if (StringManipulator.CompareStrings(processedAlternateTitle, processedSpotifyTitle))
                        {
                            titleFound = true;
                        }
                        else if (StringManipulator.CompareHiragana(processedAlternateTitle, processedSpotifyTitle))
                        {
                            titleFound = true;
                        }
                    }
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
    
    public void ExportUpdatedAt(Dictionary<int, string> updatedAt, string saveName, string fileExtension)
    {
        string savedListPath = Path.Combine(Application.persistentDataPath, $"{saveName}UpdatedAt.{fileExtension}");
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string serializedList = JsonSerializer.Serialize(updatedAt, jsonSerializerOptions);

        File.WriteAllText(savedListPath, serializedList, Encoding.Unicode);
        NativeFilePicker.ExportFile(savedListPath);
    }

    public async Task GetStats(Dictionary<int, Theme> openingThemes)
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

            titleFound = await VerifyTitle(kvp.Value.SongInfo.MALSongInfo, kvp.Value.SongInfo.SpotifySongInfo);

            if (titleFound == false)
            {
                titleMismatches++;

                string processedStr1 = StringManipulator.ProcessString(kvp.Value.SongInfo.MALSongInfo.Titles[0]);
                string processedStr2 = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Title);
            }

            artistFound = VerifyArtist(kvp.Value.SongInfo.MALSongInfo.Artists, kvp.Value.SongInfo.SpotifySongInfo.Artist);

            if (artistFound == false)
            {
                artistMismatches++;

                string processedStr1 = StringManipulator.ProcessString(kvp.Value.SongInfo.MALSongInfo.Artists[0]);
                string processedStr2 = StringManipulator.ProcessString(kvp.Value.SongInfo.SpotifySongInfo.Artist);
            }

            if (titleFound && artistFound)
            {
                verifiedSongs.Add(kvp.Key, kvp.Value);
            }
            else if (titleFound == false && artistFound == true)
            {
                rejectedTitles.Add(kvp.Key, kvp.Value);
            }
            else if (titleFound == true && artistFound == false)
            {
                rejectedArtists.Add(kvp.Key, kvp.Value);
            }
            else
            {
                completeRejections.Add(kvp.Key, kvp.Value);
            }
        }

        // ExportSongList(verifiedSongs, "VerifiedSongs", "txt");
        // ExportSongList(rejectedTitles, "RejectedTitles", "txt");
        // ExportSongList(rejectedArtists, "RejectedArtists", "txt");
        // ExportSongList(completeRejections, "CompleteRejections", "txt");

        float a = (float)titleMismatches / openingThemes.Count * 100;
        float b = (float)artistMismatches / openingThemes.Count * 100;

        Debug.Log($"Title: {titleMismatches}: {a}%"); // 67.65% // 61.41%      //51.97% //31.3% //30.71%    //29.72%    //28.74
        Debug.Log($"Artist: {artistMismatches}: {b}%"); // 55.22%   // 56.89%      //56.89 //46.26%                     //39.37 //33.07 //32.87 //28.14 //27.36

        Debug.Log("Titles:");
        foreach (KeyValuePair<int, Theme> kvp in rejectedTitles)
        {
            Debug.Log($"    {kvp.Key}");
        }

        Debug.Log("Artists:");
        foreach (KeyValuePair<int, Theme> kvp in rejectedArtists)
        {
            Debug.Log($"    {kvp.Key}");
        }

        Debug.Log("Both:");
        foreach (KeyValuePair<int, Theme> kvp in completeRejections)
        {
            Debug.Log($"    {kvp.Key}");
        }
    }
}
