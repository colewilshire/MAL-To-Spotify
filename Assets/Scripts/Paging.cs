using System;
using System.Text.Json.Serialization;

[Serializable]
public class Paging
{
    [JsonPropertyName("next")]
    public string Next { get; set; }
}