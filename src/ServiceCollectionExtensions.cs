using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp.Chunker;
using SimpleRag.DataSources.Markdown.Chunker;
using SimpleRag.DataSources.Pdf.Chunker;
using SimpleRag.DataSources.PowerPoint.Chunker;
using SimpleRag.DataSources.Word.Chunker;
using SimpleRag.VectorStorage;
using SimpleRag.VectorStorage.Models;

namespace SimpleRag;

/// <summary>
/// Extension methods for registering SimpleRag services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers SimpleRag services without GitHub integration.
    /// </summary>
    [PublicAPI]
    public static void AddSimpleRag<T>(
        this IServiceCollection services,
        VectorStoreConfiguration configuration,
        Func<IServiceProvider, T>? vectorStoreFactory) where T : Microsoft.Extensions.VectorData.VectorStore
    {
        services.AddMemoryCache();
        services.AddScoped<IVectorStoreQuery, VectorStoreQuery>();
        services.AddScoped<IVectorStoreCommand, VectorStoreCommand>();
        services.AddSingleton(configuration);
        if (vectorStoreFactory != null)
        {
            services.AddScoped<Microsoft.Extensions.VectorData.VectorStore, T>(vectorStoreFactory);
        }

        services.AddScoped<ICSharpChunker, CSharpChunker>();
        services.AddScoped<IMarkdownChunker, MarkdownChunker>();
#pragma warning disable RAG003
        services.AddScoped<IPdfChunker, PdfChunker>();
#pragma warning restore RAG003
#pragma warning disable RAG002
        services.AddScoped<IPowerPointChunker, PowerPointChunker>();
#pragma warning restore RAG002
#pragma warning disable RAG001
        services.AddScoped<IWordChunker, WordChunker>();
#pragma warning restore RAG001
        services.AddScoped<Ingestion>();
        services.AddScoped<Search>();
        services.AddScoped<DataManagement>();
    }
}