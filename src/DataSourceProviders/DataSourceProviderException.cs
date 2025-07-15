namespace SimpleRag.DataSourceProviders;

/// <summary>
/// Exception thrown when file content retrieval fails.
/// </summary>
public class DataSourceProviderException(string message, Exception? innerException = null) : Exception(message, innerException);