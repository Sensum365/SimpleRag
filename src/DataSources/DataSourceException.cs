namespace SimpleRag.DataSources;

/// <summary>
/// Exception thrown when a data source is misconfigured.
/// </summary>
public class SourceException(string message, Exception? innerException = null) : Exception(message, innerException);