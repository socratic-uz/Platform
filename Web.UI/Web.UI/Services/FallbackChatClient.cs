using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Web.UI.Services
{
    public class FallbackChatClient : IChatClient
    {
        private readonly IChatClient? _innerClient;

        public FallbackChatClient(IChatClient? innerClient = null)
        {
            _innerClient = innerClient;
        }

        public ChatClientMetadata Metadata => _innerClient?.GetService<ChatClientMetadata>() ?? new ChatClientMetadata("SocraticFallbackProvider", new Uri("https://socratic.uz"), "socratic-architect-v1");

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            if (serviceType == typeof(ChatClientMetadata)) return Metadata;
            return _innerClient?.GetService(serviceType, serviceKey) ?? (serviceType.IsInstanceOfType(this) ? this : null);
        }

        public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (_innerClient != null)
            {
                try
                {
                    return await _innerClient.GetResponseAsync(messages, options, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FallbackChatClient Error] InnerClient GetResponseAsync failed: {ex.Message}");
                }
            }

            var prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
            var text = GenerateOfflineResponse(prompt);
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, text));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            bool innerFailed = false;

            if (_innerClient != null)
            {
                IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
                try
                {
                    var stream = _innerClient.GetStreamingResponseAsync(messages, options, cancellationToken);
                    enumerator = stream.GetAsyncEnumerator(cancellationToken);
                }
                catch (Exception)
                {
                    innerFailed = true;
                }

                if (!innerFailed && enumerator != null)
                {
                    bool hasItems = false;
                    while (true)
                    {
                        ChatResponseUpdate? item = null;
                        try
                        {
                            if (!await enumerator.MoveNextAsync()) break;
                            item = enumerator.Current;
                            hasItems = true;
                        }
                        catch (Exception)
                        {
                            innerFailed = true;
                            break;
                        }

                        if (item != null)
                        {
                            yield return item;
                        }
                    }

                    await enumerator.DisposeAsync();
                    if (!innerFailed && hasItems)
                    {
                        yield break;
                    }
                }
            }

            // Fallback: Generate intelligent Socratic Architecture offline response
            var lastUserMsg = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
            var offlineMarkdown = GenerateOfflineResponse(lastUserMsg);

            // Stream response in small chunks for smooth typewriter UX
            var words = offlineMarkdown.Split(' ');
            foreach (var word in words)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new ChatResponseUpdate
                {
                    Role = ChatRole.Assistant,
                    Contents = [new TextContent(word + " ")]
                };
                await Task.Delay(15, cancellationToken);
            }
        }

        public void Dispose()
        {
            _innerClient?.Dispose();
        }

        private static string GenerateOfflineResponse(string userPrompt)
        {
            var p = userPrompt.ToLowerInvariant();

            if (p.Contains("28") || p.Contains("ux") || p.Contains("режим") || p.Contains("mode"))
            {
                return @"### 🌐 28 Бизнес-сценариев платформы Socratic

Платформа Socratic поддерживает 28 готовых моделей продаж и взаимодействия для любого бизнеса:

1. **🛍️ Классический интернет-магазин** — каталог товаров, корзина, варианты доставки и оплаты.
2. **🔨 Аукционы и торги** — ставки в реальном времени, таймер завершения.
3. **👥 Групповые совместные покупки** — скидки при наборе нужного числа участников.
4. **📅 Онлайн-запись и бронирование** — выбор даты и времени для салонов, клиник и специалистов.
5. **🎟️ Выбор мест на схеме** — интерактивная рассадка для залов, кинотеатров и стадионов.
6. **🏨 Бронирование отелей и апартаментов** — заезд/выезд, категории номеров и допуслуги.
7. **🚗 Прокат и аренда транспорта** — выбор авто/байков, точки выдачи и возврата.
8. **🚕 Такси и логистические маршруты** — расчёт стоимости поездки от точки А до Б.
9. **🔄 Подписки и регулярные платежи** — автоматическое продление доступа и услуг.
10. **❤️ Донаты и сбор средств** — пожертвования произвольной суммы в один клик.
11. **📐 Индивидуальный расчет (Смета)** — калькулятор нестандартных заказов и услуг.
12. **🧩 Конструктор товаров** — кастомизация (компьютеры, пицца, подарки).
13. **🚚 Доставка по интервалам** — выбор удобного временного окна курьера.
14. **⚡ Цифровые товары** — моментальная выдача файлов, ключей и сертификатов.
15. **💳 Подарочные сертификаты** — электронные и именные карты с номиналом.
16. **📢 Доска объявлений (P2P)** — частные объявления, услуги мастеров с безопасной сделкой.
17. **✈️ Туры и путевки** — пакетные туры с расписанием и гидами.
18. **📊 Кредит и рассрочка** — мгновенный калькулятор условий и первый взнос.
19. **📝 Заявки и анкеты** — сбор структурированных данных клиентов.
20. **🎫 Электронные билеты** — продажа билетов на мероприятия с валидацией по QR-коду.
21. **⏳ Предзаказ (Pre-Order)** — бронирование новинок до официального старта продаж.
22. **👑 Закрытый клуб** — доступ по клубным картам и членству.
23. **💡 Краудфандинг** — коллективное финансирование стартапов и проектов.
24. **💱 P2P Обмен** — безопасный обмен активами и взаиморасчеты.
25. **🍽️ Бронь столиков** — выбор столика и времени в ресторанах и кафе.
26. **🎟️ Абонементы** — пакеты посещений с контролем остатка.
27. **🔄 Trade-In** — оценка и обмен старого товара на новый со скидкой.
28. **☁️ Облачные интеграции** — подключение сторонних API и SaaS.

*Выберите любой режим для запуска продаж в вашем бизнесе!*";
            }

            if (p.Contains("привет") || p.Contains("здравствуй") || p.Contains("hello") || p.Contains("start"))
            {
                return "Здравствуйте! Я интеллектуальный помощник платформы **Socratic**. Готов помочь с покупками, выбором услуг или настройкой продаж для вашего бизнеса. Чем могу помочь?";
            }

            return @"### 🚀 Интеллектуальный помощник Socratic

Добро пожаловать в единую систему торговли и обслуживания **Socratic**!

**Чем я могу вам помочь:**
- 🛒 **Для покупателей:** поиск и подбор товаров, бронирование услуг, билетов, отелей и столиков, помощь с корзиной и способами оплаты.
- 💼 **Для владельцев бизнеса:** запуск нужной модели продаж из 28 готовых сценариев (от магазина до аукциона и аренды), подключение смарт-киосков и автоматизация торговли.
- 🧑‍💼 **Для сотрудников:** работа с заказами, кассой, каталогом товаров и POS-терминалами.

*Напишите ваш вопрос или задачу, и я с радостью помогу!*";
        }
    }
}
