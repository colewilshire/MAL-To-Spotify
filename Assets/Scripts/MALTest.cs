using JikanDotNet;
using UnityEngine;

public class MALTest : MonoBehaviour
{
    private async void Start()
    {
        IJikan jikan = new Jikan();
        var anime = await jikan.GetAnimeAsync(1);   // Cowboy Bebop

        foreach (TitleEntry titleEntry in anime.Data.Titles)
        {
            Debug.Log(titleEntry.Title);
        }
    }
}
