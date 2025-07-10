namespace SimpleRag.FileContent.Models;

/// <summary>
/// Represents the content of a file.
/// </summary>
/// <param name="Path">The full path to the file.</param>
/// <param name="Content">The file content.</param>
/// <param name="PathWithoutRoot">The path relative to the configured root.</param>
public record FileContent(string Path, string Content, string PathWithoutRoot);
