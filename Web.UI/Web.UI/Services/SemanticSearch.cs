#if FEATURE_CHAT
using Microsoft.Extensions.VectorData;
using Chat.Web;

namespace Web.UI.Services;

public class SemanticSearch(
    VectorStoreCollection<Guid, IngestedChunk> vectorCollection) : ISemanticSearch
{
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        var nearest = vectorCollection.SearchAsync(text, maxResults, new VectorSearchOptions<IngestedChunk>
        {
            Filter = documentIdFilter is { Length: > 0 } ? record => record.DocumentId == documentIdFilter : null,
        });

        var results = new List<SearchResult>();
        await foreach (var result in nearest)
            results.Add(new SearchResult(result.Record.DocumentId, result.Record.PageNumber, result.Record.Text));

        return results;
    }
}
#endif

