using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.Markdown;
using SimpleRag.FileContent;
using SimpleRag.Integrations.GitHub;
using SimpleRag.VectorStorage;

namespace SimpleRag.Extensions;

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
        services.AddSimpleRagWithGitHubIntegration(configuration, vectorStoreFactory, string.Empty);
    }

    /// <summary>
    /// Registers SimpleRag services including GitHub integration.
    /// </summary>
    [PublicAPI]
    public static void AddSimpleRagWithGitHubIntegration<T>(
        this IServiceCollection services,
        VectorStoreConfiguration configuration,
        Func<IServiceProvider, T>? vectorStoreFactory, string githubPatToken) where T : Microsoft.Extensions.VectorData.VectorStore
    {
        services.AddScoped<VectorStoreQuery>();
        services.AddScoped<VectorStoreCommand>();
        services.AddSingleton(configuration);
        if (vectorStoreFactory != null)
        {
            services.AddScoped<Microsoft.Extensions.VectorData.VectorStore, T>(vectorStoreFactory);
        }

        services.AddScoped<CSharpChunker>();
        services.AddScoped<CSharpDataSourceCommand>();
        services.AddScoped<MarkdownChunker>();
        services.AddScoped<MarkdownDataSourceCommand>();
        services.AddScoped<GitHubQuery>();
        services.AddScoped<FileContentGitHubQuery>();
        services.AddScoped<FileContentLocalQuery>();
        services.AddSingleton(new GitHubConnection(githubPatToken));
        services.AddScoped<Ingestion>();
        services.AddScoped<Search>();
    }
}