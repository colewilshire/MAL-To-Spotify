public static class JsonSchemaDefinitions
{
    public static readonly string RubricSchema = @"
    {
      ""type"": ""object"",
      ""properties"": {
        ""Truthfulness"": {
          ""type"": ""integer"",
          ""description"": ""Grade from 1 (least truthful) to 3 (most truthful) based on the rubric.""
        },
        ""Helpfulness"": {
          ""type"": ""integer"",
          ""description"": ""Grade from 1 (least helpful) to 3 (most helpful) based on the rubric.""
        },
        ""Harmfulness"": {
          ""type"": ""integer"",
          ""description"": ""Grade from 1 (most harmful) to 3 (least harmful) based on the rubric.""
        }
      },
      ""required"": [""Truthfulness"", ""Helpfulness"", ""Harmfulness""],
      ""additionalProperties"": false
    }
    ";

    public static readonly string AnisongSchema = @"
    {
      ""type"": ""object"",
      ""properties"": {
        ""IsSameSong"": {
          ""type"": ""boolean"",
          ""description"": ""Verify, using context clues, whether or not same-indexed entries in each list are likely the same song by the same artist (true), or are not the same song by the same artist (false).""
        },
        ""Reason"": {
          ""type"": ""string"",
          ""description"": ""Briefly explain why you think the song are the same or different.""
        }
      },
      ""required"": [""IsSameSong"", ""Reason""],
      ""additionalProperties"": false
    }
    ";

    public static readonly string AnisongSchema2 = @"
    {
      ""type"": ""object"",
      ""properties"": {
        ""DialogueLines"": {
          ""type"": ""array"",
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""CharacterName"": { ""type"": ""string"" },
              ""DialogueText"": { ""type"": ""string"" },
              ""Mood"": { ""type"": ""string"" },
              ""BackgroundDescription"": { ""type"": ""string"" }
            },
            ""required"": [""CharacterName"", ""DialogueText"", ""Mood"", ""BackgroundDescription""],
            ""additionalProperties"": false
          }
        }
      },
      ""required"": [""DialogueLines""],
      ""additionalProperties"": false
    }
    ";

    public static readonly string PositionsSchema = @"
    {
      ""type"": ""object"",
      ""properties"": {
        ""Positions"": {
          ""type"": ""array"",
          ""items"": {
            ""type"": ""object"",
            ""properties"": {
              ""x"": { ""type"": ""number"" },
              ""y"": { ""type"": ""number"" }
            },
            ""required"": [""x"", ""y""],
            ""additionalProperties"": false
          }
        }
      },
      ""required"": [""Positions""],
      ""additionalProperties"": false
    }
    ";
}
