namespace Big.Data.DataProcessor.Models.Configuration;

public class MongoDbSettings
{
    public required string Uri { get; set; }
    public required string DatabaseName { get; set; }
}
