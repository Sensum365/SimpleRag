using JetBrains.Annotations;

namespace SimpleRag.VectorStorage.Models;

/// <summary>
/// Configuration for connecting to the vector store.
/// </summary>
/// <param name="CollectionName">The name of the collection.</param>
/// <param name="MaxRecordSearch">The optional maximum number of records to search.</param>
[PublicAPI]
public record VectorStoreConfiguration(string CollectionName, int? MaxRecordSearch = null);