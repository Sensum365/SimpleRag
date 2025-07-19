using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.CSharp.Chunker;
using SimpleRag.DataSources.Markdown;
using SimpleRag.DataSources.Markdown.Chunker;
using SimpleRag.Integrations.GitHub;
using SimpleRag.Integrations.GitHub.Models;
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
    }
}