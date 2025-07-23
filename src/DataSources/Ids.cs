namespace SimpleRag.DataSources;

/// <summary>
/// Represent a CollectionId
/// </summary>
/// <param name="Value">The string-based value</param>
public readonly record struct CollectionId(string Value);

/// <summary>
/// Represent a SourceId
/// </summary>
/// <param name="Value">The string-based value</param>
public readonly record struct SourceId(string Value);