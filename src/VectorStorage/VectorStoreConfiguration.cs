namespace SimpleRag.VectorStorage;

public record VectorStoreConfiguration(string CollectionName, int? MaxRecordSearch = null);