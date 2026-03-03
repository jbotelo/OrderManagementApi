namespace Orm.Infrastructure.Cosmos;

public class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "OrderManagement";
    public string ContainerName { get; set; } = "Orders";
}
