using System.IO;
using Microsoft.SemanticKernel.Text;

namespace Web.UI.Services.Ingestion;

public class MarkdownDirectorySource : IIngestionSource
{
    private readonly string _sourceDirectory;
    private readonly bool _searchSubdirectories;

    public MarkdownDirectorySource(string sourceDirectory, bool searchSubdirectories = true)
    {
        _sourceDirectory = sourceDirectory;
        _searchSubdirectories = searchSubdirectories;
    }

    public static string SourceFileId(string path) => Path.GetFileName(path);
    public static string SourceFileVersion(string path) => File.GetLastWriteTimeUtc(path).ToString("o");

    public string SourceId => $"{nameof(MarkdownDirectorySource)}:{_sourceDirectory}";

    public Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IReadOnlyList<IngestedDocument> existingDocuments)
    {
        var results = new List<IngestedDocument>();
        if (!Directory.Exists(_sourceDirectory))
        {
            return Task.FromResult<IEnumerable<IngestedDocument>>(results);
        }

        var searchOption = _searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var sourceFiles = Directory.GetFiles(_sourceDirectory, "*.md", searchOption);
        var existingDocumentsById = existingDocuments.ToDictionary(d => d.DocumentId);

        foreach (var sourceFile in sourceFiles)
        {
            var relativePath = Path.GetRelativePath(_sourceDirectory, sourceFile).Replace('\\', '/');
            var sourceFileVersion = SourceFileVersion(sourceFile);
            var existingDocumentVersion = existingDocumentsById.TryGetValue(relativePath, out var existingDocument) ? existingDocument.DocumentVersion : null;
            if (existingDocumentVersion != sourceFileVersion)
            {
                results.Add(new() { Key = Guid.CreateVersion7(), SourceId = SourceId, DocumentId = relativePath, DocumentVersion = sourceFileVersion });
            }
        }

        return Task.FromResult<IEnumerable<IngestedDocument>>(results);
    }

    public Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IReadOnlyList<IngestedDocument> existingDocuments)
    {
        if (!Directory.Exists(_sourceDirectory))
        {
            return Task.FromResult<IEnumerable<IngestedDocument>>(existingDocuments);
        }

        var searchOption = _searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var currentFiles = Directory.GetFiles(_sourceDirectory, "*.md", searchOption);
        var currentFileIds = currentFiles.Select(f => Path.GetRelativePath(_sourceDirectory, f).Replace('\\', '/')).ToHashSet();
        var deletedDocuments = existingDocuments.Where(d => d.SourceId == SourceId && !currentFileIds.Contains(d.DocumentId));
        return Task.FromResult(deletedDocuments);
    }

    public Task<IEnumerable<IngestedChunk>> CreateChunksForDocumentAsync(IngestedDocument document)
    {
        var filePath = Path.Combine(_sourceDirectory, document.DocumentId);
        if (!File.Exists(filePath))
        {
            return Task.FromResult(Enumerable.Empty<IngestedChunk>());
        }

        var content = File.ReadAllText(filePath);

#pragma warning disable SKEXP0050
        var paragraphs = TextChunker.SplitMarkdownParagraphs([content], 200);
#pragma warning restore SKEXP0050

        var chunks = paragraphs.Select((text, index) => new IngestedChunk
        {
            Key = Guid.CreateVersion7(),
            DocumentId = document.DocumentId,
            PageNumber = index + 1,
            Text = text,
        });

        return Task.FromResult(chunks);
    }
}
