using System.Text;

namespace SimpleRag.DataProviders.Models;

/// <summary>
/// Represents the content of a file.
/// </summary>
/// <param name="Path">The full path to the file.</param>
/// <param name="Bytes">The file content.</param>
/// <param name="PathWithoutRoot">The path relative to the configured root.</param>
public record FileContent(string Path, byte[] Bytes, string PathWithoutRoot)
{
    /// <summary>
    /// Get the raw content as UTF8 String
    /// </summary>
    /// <returns></returns>
    public string GetContentAsUtf8String()
    {
        return Encoding.UTF8.GetString(Bytes);
    }
}