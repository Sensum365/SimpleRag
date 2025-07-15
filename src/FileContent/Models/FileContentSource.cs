using System.Text.RegularExpressions;
using SimpleRag.DataSourceProviders;

namespace SimpleRag.FileContent.Models;

/// <summary>
/// Class describing a source of file content.
/// </summary>
public class FileContentSource
{
    /// <summary>Gets or sets a value indicating whether directories are searched recursively.</summary>
    public required bool Recursive { get; init; }

    /// <summary>Gets or sets the root path.</summary>
    public required string Path { get; init; }

    /// <summary>Gets or sets patterns for files to ignore.</summary>
    public required string? FileIgnorePatterns { get; init; }

    /// <summary>
    /// The provider of the source (Local, GitHub, etc.)
    /// </summary>
    public required IDataSourceProvider Provider { get; init; }

    /// <summary>
    /// The File-extension-type to get content for
    /// </summary>
    public required string FileExtensionType { get; init; }

    /// <summary>
    /// Determines whether the specified path should be ignored.
    /// </summary>
    public bool IgnoreFile(string path)
    {
        if (string.IsNullOrWhiteSpace(FileIgnorePatterns))
        {
            return false;
        }

        string[] patternsToIgnore = FileIgnorePatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string pattern in patternsToIgnore.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}