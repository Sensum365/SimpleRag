using Microsoft.Extensions.DependencyInjection;
using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.Markdown;
using SimpleRag.FileContent;
using SimpleRag.Integrations.GitHub;
using SimpleRag.VectorStorage;

namespace SimpleRag.Extensions;

public static class ServiceCollectionExtensions
{
    //todo - add option for keyed services
    public static void AddSimpleRag<T>(
        this IServiceCollection services,
        VectorStoreConfiguration configuration,
        Func<IServiceProvider, T>? vectorStoreFactory) where T : Microsoft.Extensions.VectorData.VectorStore
    {
        services.AddSimpleRagWithGitHubIntegration(configuration, vectorStoreFactory, string.Empty);
    }

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