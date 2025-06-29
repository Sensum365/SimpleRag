using System.Text.RegularExpressions;
using SimpleRag.FileContent.Models;
using SimpleRag.Models;

namespace SimpleRag.FileContent;

public abstract class FileContentQuery : ProgressNotificationBase
{
    protected void SharedGuards(FileContentSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            throw new FileContentException("Path is not defined");
        }
    }

    protected void NotifyNumberOfFilesFound(int length)
    {
        OnNotifyProgress($"Found {length} files");
    }

    protected void NotifyIgnoredFiles(List<string> ignoredFiles)
    {
        if (ignoredFiles.Count > 0)
        {
            OnNotifyProgress($"{ignoredFiles.Count} Files Ignored");
        }
    }

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