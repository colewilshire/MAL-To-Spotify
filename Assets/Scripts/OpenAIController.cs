using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenAI.Chat;
using UnityEngine;

public class OpenAIController : Singleton<OpenAIController>
{
    [Header("API")]
    [SerializeField] private string apiKey;
    [SerializeField] private string model;

    private ChatClient client;
    private readonly List<ChatMessage> messages = new();

    protected override void Awake()
    {
        base.Awake();
        client = new ChatClient(model, apiKey);
        //await Test();
    }

    public async Task Test()
    {
        UserChatMessage message = new("How can I solve 8x + 7 = -23?");
        messages.Add(message);

        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
        messages.Add(new AssistantChatMessage(result));

        string assistantResponse = result.Value.Content[0].Text;
        Debug.Log(assistantResponse);
    }

    public async void Test2(string x, string y)
    {
        string systemPrompt = "Provided are two lists of anime song titles and their artist(s), which may have identical, transliterated, translated, or localized titles and artist names between English and Japanese. "
            + "You are tasked with verifying, using context clues, whether or not same-indexed entries in each list are likely the same song by the same artist (true), or are not the same song by the same artist (false).";

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("rubric", BinaryData.FromString(JsonSchemaDefinitions.AnisongSchema), null, true)
        };

        SystemChatMessage systemMessage = new(systemPrompt);
        UserChatMessage userMessage = new($"{x}\n{y}");

        messages.Add(systemMessage);
        messages.Add(userMessage);

        ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages, options);
        string refusal = result.Value.Content[0].Refusal;

        if (refusal != null)
        {
            Debug.Log(refusal);
            return;
        }

        messages.Add(new AssistantChatMessage(result));
        string assistantResponse = result.Value.Content[0].Text;

        Debug.Log(assistantResponse);
    }
}
