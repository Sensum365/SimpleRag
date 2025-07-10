namespace SimpleRag.FileContent;

/// <summary>
/// Exception thrown when file content retrieval fails.
/// </summary>
public class FileContentException(string message, Exception? innerException = null) : Exception(message, innerException);
