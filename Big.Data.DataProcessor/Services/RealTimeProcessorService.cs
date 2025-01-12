using Big.Data.DataProcessor.Models.DbEntities;
using Big.Data.DataProcessor.Repositories.MongoDbRepositories;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Big.Data.DataProcessor.Services;

public class RealTimeProcessorService : IRealTimeProcessorService
{
    private readonly ILogger<RealTimeProcessorService> _logger;
    private readonly ICommentsMongoDbRepository _commentsMongoDbRepository;

    public RealTimeProcessorService(ILogger<RealTimeProcessorService> logger, ICommentsMongoDbRepository commentsMongoDbRepository)
    {
        _logger = logger;
        _commentsMongoDbRepository = commentsMongoDbRepository;
    }

    public async Task StartProcessingAsync()
    {
        await _commentsMongoDbRepository.WatchCommentsAsync(ProcessChangeAsync);
    }

    public async Task ProcessChangeAsync(BsonDocument bsonDocument)
    {
        // Deserialize the BSON document to SocialMediaComment
        var comment = BsonSerializer.Deserialize<SocialMediaComment>(bsonDocument);

        // Processing logic
        _logger.LogInformation("Processing real time comment from MongoDb: {ChangeDescription}", comment.ToJson());

        await Task.CompletedTask;
    }
}
