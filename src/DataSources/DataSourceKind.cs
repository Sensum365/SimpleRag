namespace SimpleRag.DataSources;

/// <summary>
/// The Kind of Source
/// </summary>
public enum DataSourceKind
{
    /// <summary>
    /// C#
    /// </summary>
    CSharp = 1,

    /// <summary>
    /// Markdown
    /// </summary>
    Markdown = 2,

    /// <summary>
    /// PDF
    /// </summary>
    Pdf = 3,

    /// <summary>
    /// Microsoft Word
    /// </summary>
    Word = 4,

    /// <summary>
    /// Microsoft PowerPoint
    /// </summary>
    PowerPoint = 5,
}