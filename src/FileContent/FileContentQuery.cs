using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SimpleRag.FileContent.Models;
using SimpleRag.Models;

namespace SimpleRag.FileContent;

/// <summary>
/// Base class for querying file content sources.
/// </summary>
[PublicAPI]
public abstract class FileContentQuery
{
    /// <summary>
    /// Guards for input
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="FileContentException"></exception>
    protected void SharedGuards(FileContentSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new FileContentException("Path is not defined");
        }
    }

    /// <summary>
    /// Notify how many Number of files found
    /// </summary>
    /// <param name="numberOfFiles">Number of files</param>
    /// <param name="onProgressNotification">Notification</param>
    protected void NotifyNumberOfFilesFound(int numberOfFiles, Action<ProgressNotification>? onProgressNotification)
    {
        onProgressNotification?.Invoke(ProgressNotification.Create($"Found {numberOfFiles} files"));
    }

    /// <summary>
    /// Notify of what files where ignored
    /// </summary>
    /// <param name="ignoredFiles"></param>
    /// <param name="onProgressNotification">Notification</param>
    protected void NotifyIgnoredFiles(List<string> ignoredFiles, Action<ProgressNotification>? onProgressNotification)
    {
        if (ignoredFiles.Count > 0)
        {
            onProgressNotification?.Invoke(ProgressNotification.Create($"{ignoredFiles.Count} Files Ignored"));
        }
    }

    /// <summary>
    /// Determines whether the specified path should be ignored based on patterns.
    /// </summary>
    public bool IgnoreFile(string path, string fileIgnorePatterns)
    {
        if (string.IsNullOrWhiteSpace(fileIgnorePatterns))
        {
            return false;
        }

        string[] patternsToIgnore = fileIgnorePatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
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