using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Infrastructure.Services;

public static class MappedStreamHelper
{
    public static AsyncServerStreamingCall<TTarget> MapStreamCall<TSource, TTarget>(
        AsyncServerStreamingCall<TSource> call,
        Func<TSource, TTarget> mapper)
    {
        var mappedReader = new MappedAsyncStreamReader<TSource, TTarget>(call.ResponseStream, mapper);
        return new AsyncServerStreamingCall<TTarget>(
            mappedReader,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose
        );
    }
}

public class MappedAsyncStreamReader<TSource, TTarget> : IAsyncStreamReader<TTarget>
{
    private readonly IAsyncStreamReader<TSource> _reader;
    private readonly Func<TSource, TTarget> _mapper;

    public MappedAsyncStreamReader(IAsyncStreamReader<TSource> reader, Func<TSource, TTarget> mapper)
    {
        _reader = reader;
        _mapper = mapper;
    }

    public TTarget Current => _mapper(_reader.Current);

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return _reader.MoveNext(cancellationToken);
    }
}
