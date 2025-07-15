namespace SimpleRag.DataProviders.Models;

/// <summary>
/// Exception thrown when file content retrieval fails.
/// </summary>
public class DataProviderException(string message, Exception? innerException = null) : Exception(message, innerException);