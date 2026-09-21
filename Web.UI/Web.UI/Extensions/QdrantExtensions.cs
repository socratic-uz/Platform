using Aspire.Qdrant.Client;
using static SharedKernel.Options.QdrantOptions;

internal static class QdrantExtensions
{
    public static void AddQdrantClient(this IHostApplicationBuilder builder)
    {
        string connectionName = QDRANT_CONNECTION;
        if (builder.Configuration[connectionName] is var qdrantConnection && string.IsNullOrWhiteSpace(qdrantConnection))
        {
            connectionName = $"{QDRANT_CONNECTION}-LOCAL";
            qdrantConnection = builder.Configuration[connectionName]!;
        }

        builder.AddQdrantClient(connectionName, s => 
        {
            s.Key = qdrantConnection.Split(';').FirstOrDefault(s => s.StartsWith("Key="))?.Split('=')[1];
            s.Endpoint = new Uri(qdrantConnection.Split(';').FirstOrDefault(s => s.StartsWith("Endpoint="))?.Split('=')[1] ?? throw new InvalidOperationException("Qdrant endpoint is not configured."));
       
        });
    }
}