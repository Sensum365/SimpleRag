namespace SimpleRag.FileContent;

public class FileContentException(string message, Exception? innerException = null) : Exception(message, innerException);