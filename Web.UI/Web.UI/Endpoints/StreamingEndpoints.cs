#if FEATURE_FFMPEG
using System.Runtime.InteropServices;
using FFmpegProcessor.Models;
using FFmpegProcessor.Services;
using Web.UI.Hubs;
using SkiaSharp;
using Shared.Pages;

namespace Web.UI.Endpoints;

public static class StreamingEndpoints
{
    public static void MapStreamingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/streams/remotedesktop", async (
            HttpContext context,
            string sessionId,
            CancellationToken cancellationToken) =>
        {
            context.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";

            if (!RemoteSupportHub.SessionChannels.TryGetValue(sessionId, out var channel))
            {
                return Results.NotFound("Session not active");
            }

            try
            {
                await foreach (var frameBytes in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    await context.Response.Body.WriteMJpegFrameAsync(frameBytes, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal client disconnect
            }

            return Results.Ok();
        }).DisableAntiforgery();

        app.MapGet("/api/streams/video", async (
            HttpContext context,
            string source,
            double? fps,
            int? width,
            int? height,
            string? format,
            string? ffmpegPath,
            string? ffprobePath,
            IMediaStreamProcessor mediaProcessor,
            CancellationToken cancellationToken) =>
        {
            context.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";

            var options = new FFmpegStreamOptions
            {
                InputSource = source,
                Format = FrameFormat.Jpeg,
                Fps = fps > 0 ? fps : null,
                Width = width > 0 ? width : null,
                Height = height > 0 ? height : null,
                FFmpegPath = string.IsNullOrWhiteSpace(ffmpegPath) ? null : ffmpegPath,
                FFprobePath = string.IsNullOrWhiteSpace(ffprobePath) ? null : ffprobePath
            };

            try
            {
                await foreach (var frame in mediaProcessor.ReadFramesAsync(options, null, cancellationToken))
                {
                    await context.Response.Body.WriteMJpegFrameAsync(frame.Data, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }).DisableAntiforgery();

        app.MapGet("/api/streams/detector", async (
            HttpContext context,
            string source,
            double? fps,
            int? width,
            int? height,
            string? format,
            string? ffmpegPath,
            string? ffprobePath,
            IMediaStreamProcessor mediaProcessor,
            CancellationToken cancellationToken) =>
        {
            context.Response.ContentType = "multipart/x-mixed-replace; boundary=--frame";

            var options = new FFmpegStreamOptions
            {
                InputSource = source,
                Format = string.IsNullOrEmpty(format) ? FrameFormat.Jpeg : Enum.Parse<FrameFormat>(format, true),
                Fps = fps > 0 ? fps : null,
                Width = width > 0 ? width : null,
                Height = height > 0 ? height : null,
                FFmpegPath = string.IsNullOrWhiteSpace(ffmpegPath) ? null : ffmpegPath,
                FFprobePath = string.IsNullOrWhiteSpace(ffprobePath) ? null : ffprobePath
            };

            try
            {
                await foreach (var frame in mediaProcessor.ReadFramesAsync(options, null, cancellationToken))
                {
                    SKBitmap? frameBitmap = null;
                    if (frame.Format == FrameFormat.Jpeg)
                    {
                        frameBitmap = SKBitmap.Decode(frame.Data);
                    }
                    else
                    {
                        var info = new SKImageInfo(
                            frame.Width,
                            frame.Height,
                            frame.Format == FrameFormat.RawRgba ? SKColorType.Rgba8888 : SKColorType.Rgb888x
                        );
                        frameBitmap = new SKBitmap(info);

                        var gcHandle = GCHandle.Alloc(frame.Data, GCHandleType.Pinned);
                        try
                        {
                            frameBitmap.SetPixels(gcHandle.AddrOfPinnedObject());
                        }
                        finally
                        {
                            gcHandle.Free();
                        }
                    }

                    if (frameBitmap == null) continue;

                    if (Detector.yolo != null)
                    {
                        Detector.yolo.SetupYoloDefaultLabels();
                        var preds = await Detector.yolo.Predict(frameBitmap);

                        if (preds != null && preds.Count > 0)
                        {
                            using var canvas = new SKCanvas(frameBitmap);
                            using var font = new SKFont(SKTypeface.Default, 16);
                            for (int i = 0; i < preds.Count; i++)
                            {
                                if (Detector.paintRect != null)
                                {
                                    canvas.DrawRect(preds[i].Rectangle, Detector.paintRect);
                                }

                                if (Detector.paintText != null)
                                {
                                    var labelName = preds[i].Label?.Name ?? "Object";
                                    canvas.DrawText($"{labelName} ({Math.Round(preds[i].Score, 2)})",
                                        preds[i].Rectangle.Left + 8,
                                        preds[i].Rectangle.Top + 26,
                                        font,
                                        Detector.paintText);
                                }
                            }
                            canvas.Flush();
                        }
                    }

                    byte[] jpegBytes;
                    using (var image = SKImage.FromBitmap(frameBitmap))
                    using (var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 80))
                    {
                        jpegBytes = encoded.ToArray();
                    }

                    frameBitmap.Dispose();

                    // Write MJPEG frame directly using the library helper
                    await context.Response.Body.WriteMJpegFrameAsync(jpegBytes, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal client cancellation/disconnection
            }
        }).DisableAntiforgery();
    }
}
#endif

