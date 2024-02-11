using System;
using System.Text.Json.Serialization;

[Serializable]
public class MainPicture
{
    [JsonPropertyName("medium")]
    public string Medium { get; set; }

    [JsonPropertyName("large")]
    public string Large { get; set; }
}