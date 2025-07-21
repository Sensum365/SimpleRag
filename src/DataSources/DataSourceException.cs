using JetBrains.Annotations;

namespace SimpleRag.DataSources;

/// <summary>
/// Exception thrown when a data source is misconfigured.
/// </summary>
[PublicAPI]
public class DataSourceException(string message, Exception? innerException = null) : Exception(message, innerException);