#if FEATURE_CHAT
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Chat.Abstractions;

namespace Web.UI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async IAsyncEnumerable<string> StreamChat(
            List<ChatMessageDto> messages,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var cid = Context.ConnectionId;
            Console.WriteLine($"[ChatHub {DateTime.UtcNow:HH:mm:ss.fff}] StreamChat invoked by client {cid} with {messages.Count} messages.");

            int tokenCount = 0;
            await foreach (var token in _chatService.StreamResponseAsync(messages, cancellationToken))
            {
                tokenCount++;
                yield return token;
            }
            Console.WriteLine($"[ChatHub {DateTime.UtcNow:HH:mm:ss.fff}] StreamChat completed for client {cid}. Streamed {tokenCount} tokens.");
        }

        public async Task<List<string>> GetSuggestions(List<ChatMessageDto>? messages)
        {
            var cancellationToken = Context.ConnectionAborted;
            try
            {
                var list = messages ?? new List<ChatMessageDto>();
                var suggestions = await _chatService.GetSuggestionsAsync(list, cancellationToken);
                return suggestions?.ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub.GetSuggestions Error] {ex.Message}");
                return new List<string>
                {
                    "Какие есть 28 UX-режимов?",
                    "Как устроен смарт-киоск?",
                    "Каковы преимущества ScyllaDB?"
                };
            }
        }
    }
}
#endif

