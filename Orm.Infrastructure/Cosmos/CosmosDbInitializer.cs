using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Orm.Infrastructure.Cosmos;

public class CosmosDbInitializer
{
    private readonly CosmosClient _cosmosClient;
    private readonly CosmosDbSettings _settings;

    public CosmosDbInitializer(CosmosClient cosmosClient, IOptions<CosmosDbSettings> settings)
    {
        _cosmosClient = cosmosClient;
        _settings = settings.Value;
    }

    public async Task InitializeAsync()
    {
        var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_settings.DatabaseName);
        await database.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties(_settings.ContainerName, "/id"));
    }
}
