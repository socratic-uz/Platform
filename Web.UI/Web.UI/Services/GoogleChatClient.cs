using System;
using OpenAI.Chat;

namespace Web.UI.Services;

/// <summary>
/// Forwarder to Chat.Services.GoogleChatClient in Intelligence/Chat to eliminate duplication.
/// </summary>
[Obsolete("Use Chat.Services.GoogleChatClient from Intelligence/Chat instead.")]
public class GoogleChatClient : global::Chat.Services.GoogleChatClient
{
    public GoogleChatClient(ChatClient chatClient, string modelName = "gemma-4-26b-a4b-it", Uri? endpoint = null)
        : base(chatClient, modelName, endpoint)
    {
    }
}
