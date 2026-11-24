using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using SkiaSharp;
using Socratic.Vision.Features.Detection;
using Yolo;

namespace Web.UI.Endpoints;

public static class DetectorEndpoints
{
    private static readonly ConcurrentDictionary<string, WebRtcDetectorSession> _webrtcSessions = new();

    public static void MapDetectorEndpoints(this WebApplication app)
    {
        app.MapPost("/api/detector/frame", async (HttpContext ctx, IYoloDetectorService detector, CancellationToken ct) =>
        {
            var sw = Stopwatch.StartNew();
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms, ct);
            if (ms.Length == 0 || !ms.TryGetBuffer(out var seg))
                return Results.BadRequest();

            using var bitmap = SKBitmap.Decode(seg.AsSpan());
            if (bitmap == null)
                return Results.BadRequest();

            var preds = await detector.DetectAsync(bitmap, ct);
            sw.Stop();

            ctx.Response.ContentType = "application/json";
            await using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter);
            WriteDetectionJson(writer, sw.ElapsedMilliseconds, bitmap.Width, bitmap.Height, preds);
            await writer.FlushAsync(ct);
            return Results.Empty;
        }).DisableAntiforgery();

        app.MapPost("/api/detector/webrtc/offer", async (HttpContext ctx, IYoloDetectorService detector, CancellationToken ct) =>
        {
            using var doc = await JsonDocument.ParseAsync(ctx.Request.Body, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("sdp", out var sdpProp) || sdpProp.GetString() is not { Length: > 0 } offerSdp)
                return Results.BadRequest();

            var sessionId = Guid.NewGuid().ToString("N");
            var session = new WebRtcDetectorSession(sessionId, detector, () => _webrtcSessions.TryRemove(sessionId, out _));
            _webrtcSessions[sessionId] = session;

            try
            {
                var answerSdp = await session.StartAsync(offerSdp, ct);
                ctx.Response.ContentType = "application/json";
                await using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter);
                writer.WriteStartObject();
                writer.WriteString("sessionId", sessionId);
                writer.WriteString("sdp", answerSdp);
                writer.WriteEndObject();
                await writer.FlushAsync(ct);
                return Results.Empty;
            }
            catch
            {
                _webrtcSessions.TryRemove(sessionId, out _);
                session.Dispose();
                throw;
            }
        }).DisableAntiforgery();

        app.MapDelete("/api/detector/webrtc/{sessionId}", (string sessionId) =>
        {
            if (_webrtcSessions.TryRemove(sessionId, out var session))
                session.Dispose();
            return Results.Ok();
        }).DisableAntiforgery();
    }

    private static void WriteDetectionJson(Utf8JsonWriter writer, long elapsedMs, int width, int height, IReadOnlyList<YoloPrediction> preds)
    {
        writer.WriteStartObject();
        writer.WriteNumber("ms", elapsedMs);
        writer.WriteNumber("w", width);
        writer.WriteNumber("h", height);
        writer.WriteStartArray("boxes");
        for (int i = 0; i < preds.Count; i++)
        {
            var p = preds[i];
            writer.WriteStartObject();
            writer.WriteNumber("c", p.Label?.Id ?? -1);
            writer.WriteString("l", p.Label?.Name ?? string.Empty);
            writer.WriteNumber("s", MathF.Round(p.Score, 3));
            writer.WriteNumber("x", MathF.Round(p.Rectangle.Left / width * 100f, 2));
            writer.WriteNumber("y", MathF.Round(p.Rectangle.Top / height * 100f, 2));
            writer.WriteNumber("w", MathF.Round(p.Rectangle.Width / width * 100f, 2));
            writer.WriteNumber("h", MathF.Round(p.Rectangle.Height / height * 100f, 2));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private sealed class WebRtcDetectorSession : IDisposable
    {
        private readonly string _sessionId;
        private readonly IYoloDetectorService _detector;
        private readonly Action _onClosed;
        private readonly CancellationTokenSource _cts = new();
        private readonly Channel<byte[]> _decodeChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(30)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
        private readonly SemaphoreSlim _inferSignal = new(0, 1);
        private readonly VpxVideoEncoder _vpxDecoder = new();
        private SKBitmap? _latestBitmap;
        private RTCPeerConnection? _pc;
        private RTCDataChannel? _dataChannel;
        private int _disposed;

        public WebRtcDetectorSession(string sessionId, IYoloDetectorService detector, Action onClosed)
        {
            _sessionId = sessionId;
            _detector = detector;
            _onClosed = onClosed;
        }

        public async Task<string> StartAsync(string offerSdp, CancellationToken ct)
        {
            var pc = new RTCPeerConnection(new RTCConfiguration { iceServers = new List<RTCIceServer>() });
            _pc = pc;

            var videoTrack = new MediaStreamTrack(new VideoFormat(VideoCodecsEnum.VP8, 96), MediaStreamStatusEnum.RecvOnly);
            pc.addTrack(videoTrack);

            pc.ondatachannel += dc =>
            {
                _dataChannel = dc;
            };

            var iceCompleteTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            pc.onicegatheringstatechange += state =>
            {
                if (state == RTCIceGatheringState.complete)
                    iceCompleteTcs.TrySetResult();
            };

            pc.onconnectionstatechange += state =>
            {
                if (state is RTCPeerConnectionState.closed or RTCPeerConnectionState.failed or RTCPeerConnectionState.disconnected)
                {
                    _onClosed();
                    Dispose();
                }
            };

            pc.OnVideoFrameReceived += (_, _, payload, format) =>
            {
                if (_disposed == 0 && format.Codec == VideoCodecsEnum.VP8 && payload.Length > 0)
                    _decodeChannel.Writer.TryWrite(payload);
            };

            var setResult = pc.setRemoteDescription(new RTCSessionDescriptionInit { type = RTCSdpType.offer, sdp = offerSdp });
            if (setResult != SetDescriptionResultEnum.OK)
                throw new InvalidOperationException($"Invalid WebRTC offer: {setResult}");

            var answer = pc.createAnswer();
            await pc.setLocalDescription(answer);

            if (pc.iceGatheringState != RTCIceGatheringState.complete)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(1500);
                try { await iceCompleteTcs.Task.WaitAsync(timeoutCts.Token); } catch (OperationCanceledException) { }
            }

            _ = Task.Run(DecodeLoopAsync, _cts.Token);
            _ = Task.Run(InferLoopAsync, _cts.Token);

            return pc.localDescription.sdp.ToString();
        }

        private int _loggedFrameInfo;

        private async Task DecodeLoopAsync()
        {
            try
            {
                await foreach (var payload in _decodeChannel.Reader.ReadAllAsync(_cts.Token))
                {
                    foreach (var raw in _vpxDecoder.DecodeVideo(payload, VideoPixelFormatsEnum.Bgra, VideoCodecsEnum.VP8))
                    {
                        if (raw.Width == 0 || raw.Height == 0 || raw.Sample == null || raw.Sample.Length == 0)
                            continue;

                        int w = (int)raw.Width;
                        int h = (int)raw.Height;
                        if (Interlocked.Exchange(ref _loggedFrameInfo, 1) == 0)
                        {
                            Console.WriteLine($"[WebRTC VP8] Decoded VideoSample: {w}x{h}, len={raw.Sample.Length}, expectedBgra={w * h * 4}");
                        }

                        var bmp = new SKBitmap(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Opaque));
                        if (raw.Sample.Length >= w * h * 4)
                        {
                            Marshal.Copy(raw.Sample, 0, bmp.GetPixels(), w * h * 4);
                        }
                        else if (raw.Sample.Length >= w * h * 3)
                        {
                            // Convert 24-bit BGR to 32-bit BGRA
                            var src = raw.Sample;
                            var bgra = new byte[w * h * 4];
                            for (int s = 0, d = 0; d < bgra.Length && s + 2 < src.Length; s += 3, d += 4)
                            {
                                bgra[d] = src[s];
                                bgra[d + 1] = src[s + 1];
                                bgra[d + 2] = src[s + 2];
                                bgra[d + 3] = 255;
                            }
                            Marshal.Copy(bgra, 0, bmp.GetPixels(), bgra.Length);
                        }
                        else
                        {
                            bmp.Dispose();
                            continue;
                        }

                        var old = Interlocked.Exchange(ref _latestBitmap, bmp);
                        old?.Dispose();
                        if (_inferSignal.CurrentCount == 0)
                        {
                            try { _inferSignal.Release(); } catch (SemaphoreFullException) { }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }

        private int _loggedInferInfo;

        private async Task InferLoopAsync()
        {
            using var jsonBuffer = new MemoryStream(1024);
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await _inferSignal.WaitAsync(_cts.Token);
                    using var bmp = Interlocked.Exchange(ref _latestBitmap, null);
                    if (bmp == null) continue;

                    var sw = Stopwatch.StartNew();
                    var preds = await _detector.DetectAsync(bmp, _cts.Token);
                    sw.Stop();
                    if (Interlocked.Exchange(ref _loggedInferInfo, 1) == 0)
                    {
                        Console.WriteLine($"[WebRTC Infer] {bmp.Width}x{bmp.Height}, ms={sw.ElapsedMilliseconds}, preds={preds.Count}");
                    }

                    var dc = _dataChannel;
                    if (dc != null && dc.readyState == RTCDataChannelState.open)
                    {
                        jsonBuffer.SetLength(0);
                        await using (var writer = new Utf8JsonWriter(jsonBuffer))
                        {
                            WriteDetectionJson(writer, sw.ElapsedMilliseconds, bmp.Width, bmp.Height, preds);
                        }
                        dc.send(Encoding.UTF8.GetString(jsonBuffer.GetBuffer(), 0, (int)jsonBuffer.Length));
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _cts.Cancel();
            _decodeChannel.Writer.TryComplete();
            Interlocked.Exchange(ref _latestBitmap, null)?.Dispose();
            try { _pc?.close(); } catch { }
            try { _vpxDecoder.Dispose(); } catch { }
            _inferSignal.Dispose();
            _cts.Dispose();
        }
    }
}
