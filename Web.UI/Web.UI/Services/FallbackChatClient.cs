using System;
using Microsoft.Extensions.AI;

namespace Web.UI.Services;

/// <summary>
/// Forwarder to Chat.Services.FallbackChatClient in Intelligence/Chat to eliminate duplication.
/// </summary>
[Obsolete("Use Chat.Services.FallbackChatClient from Intelligence/Chat instead.")]
public class FallbackChatClient : global::Chat.Services.FallbackChatClient
{
    public FallbackChatClient(IChatClient? innerClient = null)
        : base(innerClient)
    {
    }
}
