using Big.Data.DataProcessor.Models.Configuration;
using Big.Data.DataProcessor.Models.DbEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Big.Data.DataProcessor.Repositories.MongoDbRepositories;

public class CommentsMongoDbRepository : ICommentsMongoDbRepository
{
    private readonly MongoClient _mongoClient;
    private readonly MongoDbSettings _mongoDbSettings;
    private readonly ILogger<CommentsMongoDbRepository> _logger;

    public CommentsMongoDbRepository(IOptions<MongoDbSettings> mongoSettings, ILogger<CommentsMongoDbRepository> logger)
    {
        _mongoDbSettings = mongoSettings.Value;
        var settings = MongoClientSettings.FromConnectionString(_mongoDbSettings.Uri);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        _mongoClient = new MongoClient(settings);

        _logger = logger;
    }

    public async Task WatchCommentsAsync(Func<BsonDocument, Task> processChange)
    {
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };

        var database = _mongoClient.GetDatabase(_mongoDbSettings.DatabaseName);
        var collection = database.GetCollection<BsonDocument>("Comments");

        using (var cursor = await collection.WatchAsync(options))
        {
            while (await cursor.MoveNextAsync())
            {
                foreach (var change in cursor.Current)
                {
                    await processChange(change.FullDocument);
                }
            }
        }
    }

}