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

//Setup Github Credentials (optional, but needed if you wish to RAG data directly from GitHub)
builder.Services.AddSingleton(new GitHubCredentials(githubToken));

//Setup Embedding Generator
builder.Services.AddAzureOpenAIEmbeddingGenerator(embeddingDeploymentName, endpoint, key);

//Setup SimpleRag
builder.Services.AddSimpleRag(vectorStoreConfiguration, options => new SqlServerVectorStore(sqlServerConnectionString, new SqlServerVectorStoreOptions
{
    EmbeddingGenerator = options.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
}));
```

### Step 3: Ingest Data into your VectorStore (in this sample the [TrelloDotNet](https://github.com/rwjdk/TrelloDotNet) open-source repo)
```csharp
public class IngestionExample(Ingestion ingestion, IServiceProvider serviceProvider)
{
    public async Task Sync()
    {
        DataSourceProviderGitHub filesProvider = new(serviceProvider)
        {
            GitHubRepository = new()
            {
                Owner = "rwjdk",
                Name = "TrelloDotNet"
            }
        };

        await ingestion.IngestAsync(
        [
            new CSharpDataSource(serviceProvider)
            {
                CollectionId = VectorStoreIds.CollectionId,
                Id = VectorStoreIds.SourceIdCode,
                Recursive = true,
                Path = "src",
                FileIgnorePatterns = "TrelloDotNet.Tests",
                FilesProvider = filesProvider
            },
            new MarkdownDataSource(serviceProvider)
            {
                CollectionId = VectorStoreIds.CollectionId,
                Id = VectorStoreIds.SourceIdMarkdownInCode,
                Recursive = true,
                Path = "/",
                FilesProvider = filesProvider
                LevelsToChunk = 3,
            }
        ], new IngestionOptions
        {
            OnProgressNotification = notification => Console.WriteLine(notification.GetFormattedMessageWithDetails()),
        }, cancellationToken);
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
            Filter = entity => entity.SourceCollectionId == VectorStoreIds.CollectionId
        });
        
        return searchResult.GetAsStringResult();
    }
}
```

## Legal Stuff
- PDF Extraction is done with Nuget Package [PDFPig](https://github.com/UglyToad/PdfPig/blob/master/LICENSE) (Apache 2.0 License)
- Retry Logic is provided by Nuget Package [Polly](https://github.com/App-vNext/Polly/blob/main/LICENSE) (BSD-3)