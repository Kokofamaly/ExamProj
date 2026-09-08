namespace WordCardsApi.Infrastructure.Settings;

public class MongoDbSettings
{
    public string ConnectionURI { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}