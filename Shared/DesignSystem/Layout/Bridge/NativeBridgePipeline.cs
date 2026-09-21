using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Shared.Serialization;

namespace Shared.Bridge
{
    /// <summary>
    /// High-throughput Zero-Copy Native IPC Bridge between C# Native Host and WebView using System.IO.Pipelines.
    /// Eliminates string allocations and dynamic JS-Runtime reflection.
    /// </summary>
    public sealed class NativeBridgePipeline
    {
        private readonly Pipe _outgoingPipe;
        private readonly Pipe _incomingPipe;
        private readonly AotJsonContext _jsonContext;

        public NativeBridgePipeline()
        {
            var options = new PipeOptions(
                pool: MemoryPool<byte>.Shared,
                readerScheduler: PipeScheduler.ThreadPool,
                writerScheduler: PipeScheduler.ThreadPool,
                pauseWriterThreshold: 64 * 1024,
                resumeWriterThreshold: 32 * 1024,
                minimumSegmentSize: 4096,
                useSynchronizationContext: false);

            _outgoingPipe = new Pipe(options);
            _incomingPipe = new Pipe(options);
            _jsonContext = AotJsonContext.Default;
        }

        public PipeWriter OutgoingWriter => _outgoingPipe.Writer;
        public PipeReader IncomingReader => _incomingPipe.Reader;

        /// <summary>
        /// Sends a strongly-typed payload over UTF-8 binary stream without dynamic reflection allocations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public async ValueTask SendCommandAsync<T>(int commandId, long sequenceId, T payload, CancellationToken ct = default)
        {
            var writer = _outgoingPipe.Writer;
            Memory<byte> memory = writer.GetMemory(18);

            // Header Frame: Magic (2B) + CmdId (4B) + SeqId (8B) + Reserved (4B)
            MemoryMarshal.Write(memory.Span[..2], (ushort)0x4150); // 'AP'
            MemoryMarshal.Write(memory.Span[2..6], commandId);
            MemoryMarshal.Write(memory.Span[6..14], sequenceId);
            MemoryMarshal.Write(memory.Span[14..18], 0);

            writer.Advance(18);

            await using (var jsonWriter = new Utf8JsonWriter(writer.AsStream(), new JsonWriterOptions { SkipValidation = true }))
            {
                JsonSerializer.Serialize(jsonWriter, payload, typeof(T), _jsonContext);
                await jsonWriter.FlushAsync(ct).ConfigureAwait(false);
            }

            await writer.FlushAsync(ct).ConfigureAwait(false);
        }
    }
}
