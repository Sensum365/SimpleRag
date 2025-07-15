using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.Markdown;
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
        services.AddScoped<IVectorStoreQuery, VectorStoreQuery>();
        services.AddScoped<IVectorStoreCommand, VectorStoreCommand>();
        services.AddSingleton(configuration);
        if (vectorStoreFactory != null)
        {
            services.AddScoped<Microsoft.Extensions.VectorData.VectorStore, T>(vectorStoreFactory);
        }

        services.AddScoped<ICSharpChunker, CSharpChunker>();
        services.AddScoped<IMarkdownChunker, MarkdownChunker>();
        services.AddScoped<IGitHubQuery, GitHubQuery>();
        services.AddSingleton(new GitHubConnection(githubPatToken));
        services.AddScoped<Ingestion>();
        services.AddScoped<Search>();
    }
}