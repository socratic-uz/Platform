using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Web.UI.Models;

namespace Web.UI.Hubs
{
    /// <summary>
    /// Рефракторизованный хаб SignalR для управления удаленной сессией поддержки и маршрутизации команд/потоков.
    /// Автоматически управляет ресурсами процессора на стороне клиента, запуская/останавливая стрим экрана.
    /// </summary>
    public class RemoteSupportHub : Hub
    {
        /// <summary>
        /// Потокобезопасная коллекция каналов с бинарными кадрами видеострима для каждой сессии.
        /// </summary>
        public static readonly ConcurrentDictionary<string, Channel<byte[]>> SessionChannels = new();

        /// <summary>
        /// Коллекция активных подключений в формате: ConnectionId -> (SessionId, Role).
        /// </summary>
        private static readonly ConcurrentDictionary<string, (string SessionId, string Role)> ActiveConnections = new();

        /// <summary>
        /// Присоединяет подключение к группе сессии и координирует запуск видеопотока.
        /// </summary>
        /// <param name="sessionId">Уникальный ID сессии поддержки.</param>
        /// <param name="role">Роль участника ("agent" для клиента, "operator" для сотрудника поддержки).</param>
        public async Task JoinSession(string sessionId, string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            ActiveConnections[Context.ConnectionId] = (sessionId, role);

            if (role.Equals("operator", StringComparison.OrdinalIgnoreCase) || 
                role.Equals("viewer", StringComparison.OrdinalIgnoreCase))
            {
                // Инициализируем очередь кадров для данной сессии
                SessionChannels.TryAdd(sessionId, Channel.CreateBounded<byte[]>(new BoundedChannelOptions(3)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                }));

                // Оповещаем Desktop-агент клиента, что оператор зашел, и нужно НАЧАТЬ трансляцию экрана
                await Clients.OthersInGroup(sessionId).SendAsync("StartCapture");
                Console.WriteLine($"[Hub] Оператор/Наблюдатель подключился к сессии '{sessionId}'. Уведомляем клиента о начале трансляции.");
            }
            else if (role.Equals("agent", StringComparison.OrdinalIgnoreCase) || 
                     role.Equals("host", StringComparison.OrdinalIgnoreCase))
            {
                // Проверяем, подключен ли уже оператор к этой сессии
                bool isOperatorOnline = false;
                foreach (var conn in ActiveConnections.Values)
                {
                    if (conn.SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase) && 
                        (conn.Role.Equals("operator", StringComparison.OrdinalIgnoreCase) || 
                         conn.Role.Equals("viewer", StringComparison.OrdinalIgnoreCase)))
                    {
                        isOperatorOnline = true;
                        break;
                    }
                }

                if (isOperatorOnline)
                {
                    // Если оператор уже на связи, заставляем агент сразу начать захват экрана
                    await Clients.Caller.SendAsync("StartCapture");
                }
                else
                {
                    Console.WriteLine($"[Hub] Клиент-хост подключился к '{sessionId}'. Стрим остановлен до подключения оператора.");
                }
            }
        }

        /// <summary>
        /// Метод вызова со стороны Desktop-агента для загрузки очередного скриншота экрана.
        /// </summary>
        public void UploadFrame(string sessionId, byte[] frameData)
        {
            if (SessionChannels.TryGetValue(sessionId, out var channel))
            {
                channel.Writer.TryWrite(frameData);
            }
        }

        /// <summary>
        /// Метод вызова со стороны оператора поддержки для отправки кликов и нажатий клавиатуры клиенту.
        /// </summary>
        public async Task SendControlCommand(string sessionId, RemoteControlCommand command)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveControlCommand", command);
            
            var xStr = command.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var yStr = command.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveControlCommandFlatString", command.Type, xStr, yStr);
        }

        /// <summary>
        /// Отправляет SDP Offer (предложение соединения) от оператора к клиенту.
        /// </summary>
        public async Task SendSdpOffer(string sessionId, string sdp)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveSdpOffer", sdp);
        }

        /// <summary>
        /// Отправляет SDP Answer (ответ на предложение) от клиента к оператору.
        /// </summary>
        public async Task SendSdpAnswer(string sessionId, string sdp)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveSdpAnswer", sdp);
        }

        /// <summary>
        /// Обменивает ICE-кандидаты для пробития NAT и установления P2P соединения.
        /// </summary>
        public async Task SendIceCandidate(string sessionId, string candidateJson)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveIceCandidate", candidateJson);
        }

        /// <summary>
        /// Запрашивает разрешение на подключение у удаленного клиента.
        /// </summary>
        public async Task RequestConnection(string sessionId)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveConnectionRequest");
        }

        /// <summary>
        /// Отправляет ответ клиента (разрешено/отклонено) оператору.
        /// </summary>
        public async Task ApproveConnection(string sessionId, bool approved)
        {
            await Clients.OthersInGroup(sessionId).SendAsync("ReceiveConnectionApproval", approved);
        }

        /// <summary>
        /// Обрабатывает отключение участника, очищает память и останавливает стрим у клиента, если ушел оператор.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ActiveConnections.TryRemove(Context.ConnectionId, out var connInfo))
            {
                if (connInfo.Role.Equals("operator", StringComparison.OrdinalIgnoreCase) || 
                    connInfo.Role.Equals("viewer", StringComparison.OrdinalIgnoreCase))
                {
                    // Агент закрыл страницу/отключился — тушим захват экрана на клиенте для экономии батареи и CPU
                    await Clients.OthersInGroup(connInfo.SessionId).SendAsync("StopCapture");
                    Console.WriteLine($"[Hub] Оператор/Наблюдатель отключился от сессии '{connInfo.SessionId}'. Даем клиенту команду ОСТАНОВИТЬ захват.");
                    
                    // Удаляем канал
                    SessionChannels.TryRemove(connInfo.SessionId, out _);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
