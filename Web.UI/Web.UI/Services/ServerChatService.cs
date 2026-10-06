#if FEATURE_CHAT
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Chat.Abstractions;

namespace Web.UI.Services
{
    public class ServerChatService : IChatService
    {
        private readonly IChatClient _chatClient;

        public ServerChatService(IServiceProvider serviceProvider)
        {
            var registeredClient = serviceProvider.GetService<IChatClient>();
            _chatClient = new global::Chat.Services.FallbackChatClient(registeredClient);
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(
            IEnumerable<ChatMessageDto> messages,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var aiMessages = messages.Select(m => new ChatMessage(
                m.Role.Equals("User", System.StringComparison.OrdinalIgnoreCase) ? ChatRole.User :
                m.Role.Equals("System", System.StringComparison.OrdinalIgnoreCase) ? ChatRole.System : ChatRole.Assistant,
                m.Content)).ToList();

            var chatOptions = new ChatOptions();

            IAsyncEnumerable<ChatResponseUpdate>? responseUpdates = null;
            string? errorMessage = null;

            try
            {
                responseUpdates = _chatClient.GetStreamingResponseAsync(aiMessages, chatOptions, cancellationToken);
            }
            catch (System.Exception ex)
            {
                errorMessage = $"⚠️ Ошибка вызова ИИ-сервиса: {ex.Message}. Пожалуйста, проверьте параметры подключения ИИ.";
            }

            if (errorMessage != null)
            {
                yield return errorMessage;
                yield break;
            }

            IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
            try
            {
                enumerator = responseUpdates!.GetAsyncEnumerator(cancellationToken);
            }
            catch (System.Exception ex)
            {
                errorMessage = $"⚠️ Ошибка ИИ-сервиса: {ex.Message}";
            }

            if (errorMessage != null)
            {
                yield return errorMessage;
                yield break;
            }

            bool hasMore = true;
            while (hasMore)
            {
                ChatResponseUpdate? update = null;
                errorMessage = null;

                try
                {
                    hasMore = await enumerator!.MoveNextAsync();
                    if (hasMore)
                    {
                        update = enumerator.Current;
                    }
                }
                catch (System.Exception ex)
                {
                    errorMessage = $"\n\n⚠️ ИИ-сервис вернул ошибку: {ex.Message}";
                    hasMore = false;
                }

                if (errorMessage != null)
                {
                    yield return errorMessage;
                    break;
                }

                if (update != null && !string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }

            if (enumerator != null)
            {
                await enumerator.DisposeAsync();
            }
        }

        public async Task<IReadOnlyList<string>> GetSuggestionsAsync(
            IEnumerable<ChatMessageDto> messages,
            CancellationToken cancellationToken = default)
        {
            var msgList = messages.ToList();
            var conversationMessages = msgList
                .Where(m => !m.Role.Equals("System", System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(m.Content))
                .TakeLast(6)
                .ToList();

            string prompt;
            if (conversationMessages.Any(m => m.Role.Equals("User", System.StringComparison.OrdinalIgnoreCase)))
            {
                var summary = string.Join("\n", conversationMessages.Select(m =>
                    $"{(m.Role.Equals("User", System.StringComparison.OrdinalIgnoreCase) ? "Пользователь" : "Ассистент")}: {m.Content}"));

                prompt = $@"Контекст недавнего диалога:
---
{summary}
---
На основе контекста выше предложи ровно 3 коротких, актуальных вопроса (до 6 слов каждый, на русском языке), которые пользователь логично захочет спросить следующими в продолжение беседы.
Каждый вопрос должен быть кратким вопросительным предложением.
Верни ТОЛЬКО 3 вопроса, каждый с новой строки, без цифр, без маркеров, без лишних пояснений.";
            }
            else
            {
                prompt = @"Предложи ровно 3 интересных стартовых вопроса (до 6 слов каждый, на русском языке) о возможностях платформы Socratic для бизнеса и покупателей (например, о 28 UX-режимах, смарт-киосках, ScyllaDB).
Верни ТОЛЬКО 3 вопроса, каждый с новой строки, без цифр, без маркеров, без лишних пояснений.";
            }

            try
            {
                var chatOptions = new ChatOptions
                {
                    Temperature = 0.7f,
                    MaxOutputTokens = 200
                };

                var response = await _chatClient.GetResponseAsync(
                    [new ChatMessage(ChatRole.User, prompt)],
                    chatOptions,
                    cancellationToken: cancellationToken);

                var rawText = response.Messages.LastOrDefault()?.Text ?? "";
                var parsed = ParseSuggestions(rawText);
                if (parsed != null && parsed.Length > 0)
                {
                    return parsed;
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[GetSuggestionsAsync Error] {ex.Message}");
            }

            return GetFallbackSuggestions(conversationMessages);
        }

        private static string[]? ParseSuggestions(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return null;

            var clean = System.Text.RegularExpressions.Regex.Replace(rawText, @"<thought>[\s\S]*?</thought>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

            var jsonMatch = System.Text.RegularExpressions.Regex.Match(clean, @"\[\s*""[\s\S]*?""\s*\]");
            if (jsonMatch.Success)
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize(jsonMatch.Value, Shared.Serialization.AotJsonContext.Default.StringArray);
                    if (parsed != null && parsed.Length > 0)
                    {
                        return parsed.Where(s => !string.IsNullOrWhiteSpace(s)).Take(3).ToArray();
                    }
                }
                catch { }
            }

            var lines = clean.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim().Trim('`', '"', '\''))
                .Select(l => System.Text.RegularExpressions.Regex.Replace(l, @"^(\d+[\.\)]|\*|\-)\s*", "").Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && l.Length > 3 && !l.StartsWith("[") && !l.StartsWith("]"))
                .Take(3)
                .ToArray();

            return lines.Length > 0 ? lines : null;
        }

        private static string[] GetFallbackSuggestions(IReadOnlyList<ChatMessageDto>? conversationMessages = null)
        {
            if (conversationMessages != null && conversationMessages.Count > 0)
            {
                var combined = string.Join(" ", conversationMessages.Select(m => m.Content)).ToLowerInvariant();
                if (combined.Contains("заказ") || combined.Contains("корзин") || combined.Contains("доставк") || combined.Contains("оплат"))
                {
                    return new[]
                    {
                        "Как оформить доставку курьером?",
                        "Какие способы оплаты поддерживаются?",
                        "Как отследить статус заказа?"
                    };
                }
                if (combined.Contains("отел") || combined.Contains("номер") || combined.Contains("бронь") || combined.Contains("заезд"))
                {
                    return new[]
                    {
                        "Как выбрать категорию номера?",
                        "Включен ли завтрак в стоимость?",
                        "Как работает отмена бронирования?"
                    };
                }
                if (combined.Contains("киоск") || combined.Contains("pos") || combined.Contains("касс") || combined.Contains("чек"))
                {
                    return new[]
                    {
                        "Как подключить QR-чеки Soliq?",
                        "Как настроить термопринтер чеков?",
                        "Как работает кассовый режим POS?"
                    };
                }
                if (combined.Contains("мест") || combined.Contains("зал") || combined.Contains("билет") || combined.Contains("схем"))
                {
                    return new[]
                    {
                        "Как выбрать место на схеме зала?",
                        "Как получить электронный QR-билет?",
                        "Есть ли скидки для групповых заказов?"
                    };
                }
                if (combined.Contains("scylla") || combined.Contains("бд") || combined.Contains("баз") || combined.Contains("архитектур"))
                {
                    return new[]
                    {
                        "Как устроен partition key в ScyllaDB?",
                        "Почему EF Core запрещен в системе?",
                        "Как реплицируются данные в кластере?"
                    };
                }
                if (combined.Contains("28") || combined.Contains("ux") || combined.Contains("режим"))
                {
                    return new[]
                    {
                        "Как запустить режим аукциона?",
                        "Как работает аренда транспорта?",
                        "Как подключить подписки на услуги?"
                    };
                }
                if (combined.Contains("товар") || combined.Contains("каталог") || combined.Contains("цена") || combined.Contains("купит"))
                {
                    return new[]
                    {
                        "Как добавить товар в корзину?",
                        "Есть ли сезонные скидки?",
                        "Как применить промокод при покупке?"
                    };
                }
            }

            return new[]
            {
                "Какие есть 28 UX-режимов?",
                "Как устроен смарт-киоск?",
                "Каковы преимущества ScyllaDB?"
            };
        }
    }
}
#endif
