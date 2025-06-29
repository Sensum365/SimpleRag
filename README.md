> [!Caution]
> This Repo is in an Early Preview state, meaning that everything is subject to change

[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/sensum365/SimpleRag/Build.yml?style=for-the-badge)](https://github.com/sensum365/SimpleRag/actions)
[![GitHub Issues or Pull Requests by label](https://img.shields.io/github/issues/sensum365/SimpleRag/bug?style=for-the-badge&label=Bugs)](https://github.com/sensum365/SimpleRag/issues?q=is%3Aissue%20state%3Aopen%20label%3Abug)
[![Libraries.io dependency status for GitHub repo](https://img.shields.io/librariesio/github/sensum365/SimpleRag?style=for-the-badge)](https://github.com/sensum365/SimpleRag/network/dependencies)


# SimpleRag
A Simple and Opinionated AI RAG Solutions for SourceCode, where you just need to setup and point to your source and we take care of the rest.

[![NuGet](https://img.shields.io/badge/NuGet-blue?style=for-the-badge)](https://www.nuget.org/packages/SimpleRag)

## Getting Started

### Step 1: Install SimpleRag Nuget + your Embedding Generator (in this sample Azure OpenAI via Semantic Kernel and SQL Server 2025 as VectorStore)
```bash
dotnet add package SimpleRag
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Connectors.SqlServer
```

### Step 2: Setup Dependency Injection of Embedding Generator and SimpleRag
```csharp
string endpoint = builder.Configuration["AiEndpoint"]!;
string key = builder.Configuration["AiKey"]!;
string embeddingDeploymentName = builder.Configuration["AiEmbeddingDeploymentName"]!;
string sqlServerConnectionString = builder.Configuration["SqlServerConnectionString"]!;
string githubToken = builder.Configuration["GitHubToken"]!;

//Setup Embedding Generator
builder.Services.AddAzureOpenAIEmbeddingGenerator(embeddingDeploymentName, endpoint, key);

//Setup SimpleRag (use .AddSimpleRag(...) instead if you do not wish to use GitHub as Datasource)
builder.Services.AddSimpleRagWithGithubIntegration(new VectorStoreConfiguration(Constants.VectorStoreName, Constants.MaxRecords), options => new SqlServerVectorStore(sqlServerConnectionString, new SqlServerVectorStoreOptions
{
    EmbeddingGenerator = options.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
}), githubToken);
```

### Step 3: Ingest Data into your VectorStore (in this sample the [TrelloDotNet](https://github.com/rwjdk/TrelloDotNet) open-source repo)
```csharp
public class IngestionExample(Ingestion ingestion)
{
    public async Task Sync()
    {
        string gitHubOwner = "rwjdk";
        string gitHubRepo = "TrelloDotNet";

        ingestion.NotifyProgress += NotifyProgress;

        await ingestion.IngestAsync(
        [
            new CSharpDataSourceGitHub
            {
                CollectionId = Constants.CollectionId,
                Id = Constants.SourceIdCode,
                Recursive = true,
                Path = "src",
                FileIgnorePatterns = "TrelloDotNet.Tests",
                GitHubOwner = gitHubOwner,
                GitHubRepo = gitHubRepo,
            },
            new MarkdownDataSourceGitHub
            {
                CollectionId = Constants.CollectionId,
                Id = Constants.SourceIdMarkdownInCode,
                Recursive = true,
                Path = "/",
                GitHubOwner = gitHubOwner,
                GitHubRepo = gitHubRepo,
                LevelsToChunk = 3,
            }
        ]);
    }

    private void NotifyProgress(ProgressNotification obj)
    {
        Console.WriteLine(obj.GetFormattedMessageWithDetails());
    }
}
```

### Step 4: Search your VectorStore
```csharp
public class SearchExample(Search search)
{
    public async Task<string> SearchAsync(string question)
    {
        SearchResult searchResult = await search.SearchAsync(new SearchOptions
        {
            SearchQuery = searchQuery,
            NumberOfRecordsBack = 10,
            Filter = entity => entity.SourceCollectionId == Constants.CollectionId
        });
        return searchResult.GetAsStringResult();
    }
}
```
