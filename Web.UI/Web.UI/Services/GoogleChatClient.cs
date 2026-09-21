using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Web.UI.Services;

public class GoogleChatClient : IChatClient
{
    private readonly ChatClient _chatClient;
    private readonly string _modelName;
    private readonly Uri _endpoint;
    private readonly bool _isGemmaModel;

    public GoogleChatClient(ChatClient chatClient, string modelName = "gemma-4-26b-a4b-it", Uri? endpoint = null)
    {
        _chatClient = chatClient;
        _modelName = modelName;
        _endpoint = endpoint ?? new Uri("https://generativelanguage.googleapis.com/v1beta/openai/");
        _isGemmaModel = modelName.Contains("gemma", StringComparison.OrdinalIgnoreCase);
    }

    public ChatClientMetadata Metadata => new ChatClientMetadata("Google", _endpoint, _modelName);

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var sb = new System.Text.StringBuilder();
        await foreach (var update in GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            if (update.Contents != null)
            {
                foreach (var content in update.Contents)
                {
                    if (content is TextContent tc && !string.IsNullOrEmpty(tc.Text))
                    {
                        sb.Append(tc.Text);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
            }
        }

        var replyText = sb.ToString();
        if (_isGemmaModel)
        {
            replyText = StripThoughtBlocks(replyText);
        }
        return new ChatResponse(new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, replyText));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var openAiMessages = ConvertMessages(messages);
        var updates = _chatClient.CompleteChatStreamingAsync(openAiMessages, cancellationToken: cancellationToken);

        if (!_isGemmaModel)
        {
            await foreach (var update in updates)
            {
                foreach (var part in update.ContentUpdate)
                {
                    if (string.IsNullOrEmpty(part.Text)) continue;
                    yield return new ChatResponseUpdate
                    {
                        Role = ChatRole.Assistant,
                        Contents = [new TextContent(part.Text)]
                    };
                }
            }
            yield break;
        }

        bool inThought = false;
        var buffer = new System.Text.StringBuilder();

        await foreach (var update in updates)
        {
            foreach (var part in update.ContentUpdate)
            {
                if (string.IsNullOrEmpty(part.Text)) continue;

                var text = part.Text;
                buffer.Append(text);

                while (buffer.Length > 0)
                {
                    var currentStr = buffer.ToString();

                    if (!inThought)
                    {
                        var thoughtStart = currentStr.IndexOf("<thought>", StringComparison.OrdinalIgnoreCase);
                        if (thoughtStart >= 0)
                        {
                            if (thoughtStart > 0)
                            {
                                var before = currentStr.Substring(0, thoughtStart);
                                yield return new ChatResponseUpdate
                                {
                                    Role = ChatRole.Assistant,
                                    Contents = [new TextContent(before)]
                                };
                            }
                            inThought = true;
                            buffer.Remove(0, thoughtStart + 9);
                        }
                        else
                        {
                            if (currentStr.EndsWith("<") || currentStr.EndsWith("<t") || currentStr.EndsWith("<th") ||
                                currentStr.EndsWith("<tho") || currentStr.EndsWith("<thou") || currentStr.EndsWith("<thoug") ||
                                currentStr.EndsWith("<though") || currentStr.EndsWith("<thought"))
                            {
                                break;
                            }

                            yield return new ChatResponseUpdate
                            {
                                Role = ChatRole.Assistant,
                                Contents = [new TextContent(currentStr)]
                            };
                            buffer.Clear();
                        }
                    }
                    else
                    {
                        var thoughtEnd = currentStr.IndexOf("</thought>", StringComparison.OrdinalIgnoreCase);
                        if (thoughtEnd >= 0)
                        {
                            inThought = false;
                            buffer.Remove(0, thoughtEnd + 10);
                            while (buffer.Length > 0 && (buffer[0] == '\r' || buffer[0] == '\n' || buffer[0] == ' '))
                            {
                                buffer.Remove(0, 1);
                            }
                        }
                        else
                        {
                            buffer.Clear();
                            break;
                        }
                    }
                }
            }
        }

        if (!inThought && buffer.Length > 0)
        {
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(buffer.ToString())]
            };
        }
    }

    private static string StripThoughtBlocks(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"<thought>[\s\S]*?</thought>",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
    }

    public void Dispose()
    {
    }

    private static List<OpenAI.Chat.ChatMessage> ConvertMessages(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
    {
        var result = new List<OpenAI.Chat.ChatMessage>();
        foreach (var m in messages)
        {
            var text = m.Text ?? "";
            if (m.Role == ChatRole.User)
                result.Add(new UserChatMessage(text));
            else if (m.Role == ChatRole.System)
                result.Add(new SystemChatMessage(text));
            else
                result.Add(new AssistantChatMessage(text));
        }
        return result;
    }
}
