using SimpleRag.DataSources.CSharp;
using SimpleRag.DataSources.CSharp.Models;
using SimpleRag.DataSources.Markdown;
using SimpleRag.DataSources.Markdown.Models;
using SimpleRag.DataSources.Models;
using SimpleRag.Models;

namespace SimpleRag;

public class Ingestion : ProgressNotificationBase, IDisposable
{
    private readonly CSharpDataSourceCommand _cSharpDataSourceCommand;
    private readonly MarkdownDataSourceCommand _markdownDataSourceCommand;

    public Ingestion(CSharpDataSourceCommand cSharpDataSourceCommand, MarkdownDataSourceCommand markdownDataSourceCommand)
    {
        _cSharpDataSourceCommand = cSharpDataSourceCommand;
        _markdownDataSourceCommand = markdownDataSourceCommand;
        _cSharpDataSourceCommand.NotifyProgress += OnNotifyProgress;
        _markdownDataSourceCommand.NotifyProgress += OnNotifyProgress;
    }

    public async Task IngestAsync(IEnumerable<DataSource> dataSources, IngestionOptions? options = null)
    {
        foreach (DataSource source in dataSources)
        {
            switch (source)
            {
                case CSharpDataSourceLocal cSharpDataSourceLocal:
                    await _cSharpDataSourceCommand.IngestAsync(cSharpDataSourceLocal, options?.CSharpContentFormatBuilder);
                    break;
                case CSharpDataSourceGitHub cSharpDataSourceGitHub:
                    await _cSharpDataSourceCommand.IngestAsync(cSharpDataSourceGitHub, options?.CSharpContentFormatBuilder);
                    break;
                case MarkdownDataSourceLocal markdownDataSourceLocal:
                    await _markdownDataSourceCommand.IngestAsync(markdownDataSourceLocal); //todo support content format builder
                    break;
                case MarkdownDataSourceGitHub markdownDataSourceGitHub:
                    await _markdownDataSourceCommand.IngestAsync(markdownDataSourceGitHub); //todo support content format builder
                    break;
            }
        }
    }

    public void Dispose()
    {
        _cSharpDataSourceCommand.NotifyProgress -= OnNotifyProgress;
        _markdownDataSourceCommand.NotifyProgress -= OnNotifyProgress;
    }
}