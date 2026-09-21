using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Web.UI.Services
{
    public class OpenAiInferenceChatClient : IChatClient
    {
        private readonly ChatClient _chatClient;

        public OpenAiInferenceChatClient(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public ChatClientMetadata Metadata => new ChatClientMetadata("GitHubModelsInference", new Uri("https://models.inference.ai.azure.com"), "gpt-4o-mini");

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return serviceType.IsInstanceOfType(this) ? this : null;
        }

        public async Task<ChatResponse> GetResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var openAiMessages = ConvertMessages(messages);
            var result = await _chatClient.CompleteChatAsync(openAiMessages, cancellationToken: cancellationToken);
            var replyText = result.Value.Content.FirstOrDefault()?.Text ?? "";
            return new ChatResponse(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, replyText));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var openAiMessages = ConvertMessages(messages);
            var updates = _chatClient.CompleteChatStreamingAsync(openAiMessages, cancellationToken: cancellationToken);

            await foreach (var update in updates)
            {
                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        yield return new ChatResponseUpdate
                        {
                            Role = ChatRole.Assistant,
                            Contents = [new TextContent(part.Text)]
                        };
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        private static List<OpenAI.Chat.ChatMessage> ConvertMessages(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
        {
            var result = new List<OpenAI.Chat.ChatMessage>();
            foreach (var m in messages)
            {
                if (m.Role == ChatRole.User)
                    result.Add(new UserChatMessage(m.Text));
                else if (m.Role == ChatRole.System)
                    result.Add(new SystemChatMessage(m.Text));
                else
                    result.Add(new AssistantChatMessage(m.Text));
            }
            return result;
        }
    }
}
