using System;
using System.Text.Json.Serialization;

[Serializable]
public class AnimeData
{
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}