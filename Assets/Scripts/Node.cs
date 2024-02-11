using System;
using System.Text.Json.Serialization;

[Serializable]
public class Node
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("main_picture")]
    public MainPicture MainPicture { get; set; }
}