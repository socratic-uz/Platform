using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using FFmpegProcessor.Models;
using Socratic.WebRtc;
using Socratic.RemoteControl;

namespace RemoteSupport.Agent
{
    class Program
    {
        static HubConnection? connection;
        static string sessionId = "support-demo";
        static string serverBaseUrl = Environment.GetEnvironmentVariable("REMOTE_SUPPORT_SERVER_URL")?.TrimEnd('/') ?? "https://localhost:7196";
        static string serverUrl = $"{serverBaseUrl}/hubs/remotesupport";

        // WebRTC и удаленное управление состояния
        static IWebRtcConnection? peerConnection;
        static IScreenCapturer? screenCapturer;
        static IInputSimulator? inputSimulator;
        static CancellationTokenSource? captureCts;
        static readonly object captureLock = new();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("==================================================");
            Console.WriteLine("=== Remote Support Desktop Agent (WebRTC / UDP) ===");
            Console.WriteLine("==================================================");

            inputSimulator = RemoteControlFactory.CreateInputSimulator();

            if (args.Length == 0)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Предоставить доступ к моему экрану (показать этот ПК)");
                Console.WriteLine("2. Управлять другим компьютером (ввести код партнера)");
                Console.Write("Введите номер (1 или 2): ");
                
                string? choice = Console.ReadLine();
                if (choice == "2")
                {
                    Console.Write("Введите код подключения партнера: ");
                    string? code = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        Console.WriteLine("[ERROR] Код не может быть пустым.");
                        return;
                    }
                    sessionId = code.Trim();
                    
                    string targetUrl = $"{serverBaseUrl}/support/session/{Uri.EscapeDataString(sessionId)}";
                    Console.WriteLine($"[INFO] Открытие браузера для управления сессией {sessionId}...");
                    try
                    {
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {targetUrl}") { CreateNoWindow = true });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Не удалось автоматически открыть браузер: {ex.Message}");
                        Console.WriteLine($"Пожалуйста, откройте вручную: {targetUrl}");
                    }
                    Console.WriteLine("[INFO] Сессия запущена в браузере. Вы можете закрыть эту консоль.");
                    return;
                }
                else
                {
                    // Режим 1: Предоставить доступ
                    sessionId = Random.Shared.Next(1000, 9999).ToString();
                    Console.WriteLine($"[INFO] Создан код подключения: {sessionId}");
                }
            }
            else
            {
                if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                {
                    sessionId = args[0];
                }
                if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
                {
                    var customUrl = args[1].TrimEnd('/');
                    if (customUrl.EndsWith("/hubs/remotesupport", StringComparison.OrdinalIgnoreCase))
                    {
                        serverUrl = customUrl;
                        serverBaseUrl = customUrl[..^"/hubs/remotesupport".Length];
                    }
                    else
                    {
                        serverBaseUrl = customUrl;
                        serverUrl = $"{serverBaseUrl}/hubs/remotesupport";
                    }
                }
            }

            Console.WriteLine($"[INFO] ID Сессии: {sessionId}");
            Console.WriteLine($"[INFO] Адрес сервера: {serverUrl}");

            connection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .WithAutomaticReconnect()
                .Build();

            // Входящие команды ввода
            connection.On<RemoteControlCommand>("ReceiveControlCommand", (command) =>
            {
                Console.WriteLine($"[КОМАНДА] Ввод: {command.Type} | X: {command.X:F1}%, Y: {command.Y:F1}%");
                inputSimulator?.SimulateInput(command);
            });

            // Получение SDP Offer от браузера оператора
            connection.On<string>("ReceiveSdpOffer", async (sdp) =>
            {
                Console.WriteLine("[WebRTC] Получен SDP Offer от оператора. Согласование...");
                await InitializeWebRtcPeerConnection(sdp);
            });

            // Получение ICE-кандидатов от браузера оператора
            connection.On<string>("ReceiveIceCandidate", (candidateJson) =>
            {
                try
                {
                    if (peerConnection != null)
                    {
                        peerConnection.AddIceCandidate(candidateJson);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WebRTC ERROR] Ошибка добавления ICE-кандидата: {ex.Message}");
                }
            });

            // Сигнал НАЧАТЬ трансляцию (когда оператор онлайн)
            connection.On("StartCapture", () =>
            {
                Console.WriteLine("[STREAM] Оператор онлайн. Ожидание SDP инициализации...");
            });

            // Запрос на подключение от оператора (одобрение пользователя)
            connection.On("ReceiveConnectionRequest", async () =>
            {
                Console.Beep();
                Console.WriteLine();
                Console.WriteLine("==================================================");
                Console.WriteLine("[ВНИМАНИЕ] Оператор запрашивает доступ к вашему экрану.");
                Console.Write("Разрешить удаленное управление вашим ПК? (y/n): ");
                
                string? response = await Task.Run(() => Console.ReadLine());
                bool approved = response != null && (response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) || response.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase));
                
                if (approved)
                {
                    Console.WriteLine("[INFO] Соединение разрешено. Отправка подтверждения...");
                    await connection.SendAsync("ApproveConnection", sessionId, true);
                }
                else
                {
                    Console.WriteLine("[INFO] Соединение отклонено.");
                    await connection.SendAsync("ApproveConnection", sessionId, false);
                }
            });

            // Сигнал ОСТАНОВИТЬ трансляцию
            connection.On("StopCapture", () =>
            {
                lock (captureLock)
                {
                    Console.WriteLine("[STREAM] Оператор покинул сессию. Остановка стрима...");
                    StopScreenCapture();
                }
            });

            try
            {
                Console.WriteLine("[INFO] Подключение к серверу сигналинга...");
                await connection.StartAsync();
                Console.WriteLine("[SUCCESS] Успешно подключено к SignalR.");

                // Присоединяемся
                await connection.SendAsync("JoinSession", sessionId, "agent");
                Console.WriteLine($"[SUCCESS] Агент добавлен в группу '{sessionId}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка подключения к серверу: {ex.Message}");
                Console.WriteLine("[INFO] Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("[INFO] Агент готов к приему WebRTC соединений.");
            Console.WriteLine("[INFO] Нажмите Ctrl+C для выхода...");

            var shutdownCompletion = new TaskCompletionSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("\n[INFO] Завершение работы...");
                e.Cancel = true;
                lock (captureLock)
                {
                    StopScreenCapture();
                }
                shutdownCompletion.TrySetResult();
            };

            await shutdownCompletion.Task;

            try
            {
                await connection.StopAsync();
            }
            catch {}

            Console.WriteLine("[INFO] Агент успешно остановлен.");
        }

        /// <summary>
        /// Инициализирует WebRTC соединение с оператором на основе SDP предложения.
        /// </summary>
        static async Task InitializeWebRtcPeerConnection(string sdp)
        {
            lock (captureLock)
            {
                StopScreenCapture();

                peerConnection = WebRtcConnectionFactory.CreateConnection();

                peerConnection.OnIceCandidateGenerated += (json) =>
                {
                    connection?.SendAsync("SendIceCandidate", sessionId, json);
                };

                peerConnection.OnConnectionStateChanged += (stateStr) =>
                {
                    Console.WriteLine($"[WebRTC] Состояние соединения: {stateStr}");
                    if (stateStr == "closed" || stateStr == "failed")
                    {
                        lock (captureLock)
                        {
                            StopScreenCapture();
                        }
                    }
                };
            }

            try
            {
                string answerSdp = await peerConnection.InitializeAndAnswerAsync(sdp, WebRtcProfile.ScreenShare);

                // Пересылаем SDP Answer обратно оператору
                await connection!.SendAsync("SendSdpAnswer", sessionId, answerSdp);
                Console.WriteLine("[WebRTC] SDP Answer успешно отправлен оператору.");

                // Запускаем трансляцию рабочего стола
                lock (captureLock)
                {
                    screenCapturer = RemoteControlFactory.CreateScreenCapturer();
                    screenCapturer.OnFrameCaptured += (durationTicks, frameBuffer) =>
                    {
                        lock (captureLock)
                        {
                            peerConnection?.SendVideoFrame(durationTicks, frameBuffer);
                        }
                    };

                    captureCts = new CancellationTokenSource();
                    Console.WriteLine("[STREAM] Запуск трансляции экрана...");
                    screenCapturer.Start(captureCts.Token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebRTC ERROR] Ошибка согласования: {ex.Message}");
            }
        }

        static void StopScreenCapture()
        {
            if (captureCts != null)
            {
                captureCts.Cancel();
                captureCts.Dispose();
                captureCts = null;
            }

            if (screenCapturer != null)
            {
                screenCapturer.Stop();
                screenCapturer = null;
            }

            if (peerConnection != null)
            {
                peerConnection.Close();
                peerConnection = null;
            }
        }
    }
}
