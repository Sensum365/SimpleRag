namespace SimpleRag.DataSources.Models;

public class SourceException(string message, Exception? innerException = null) : Exception(message, innerException);