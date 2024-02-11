using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

[Serializable]
public class AnimeListResponse
{
    [JsonPropertyName("data")]
    public List<AnimeData> Data { get; set; }

    [JsonPropertyName("paging")]
    public Paging Paging { get; set; }
}