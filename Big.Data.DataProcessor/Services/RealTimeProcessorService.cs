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
    private readonly IPredictionService _predictionService;
    private readonly MetricsService _metricsService;

    public RealTimeProcessorService(
        ILogger<RealTimeProcessorService> logger,
        ICommentsMongoDbRepository commentsMongoDbRepository,
        IPredictionService predictionService,
        MetricsService metricsService)
    {
        _logger = logger;
        _commentsMongoDbRepository = commentsMongoDbRepository;
        _predictionService = predictionService;
        _metricsService = metricsService;
    }

    public async Task StartProcessingAsync()
    {
        await _commentsMongoDbRepository.WatchCommentsAsync(ProcessChangeAsync);
    }

    public async Task ProcessChangeAsync(BsonDocument bsonDocument)
    {
        // Deserialize the BSON document to SocialMediaComment
        var comment = BsonSerializer.Deserialize<SocialMediaComment>(bsonDocument);

        _metricsService.IncrementRealTimeProcessedComments();

        // Processing logic
        //_logger.LogInformation("Processing real time comment from MongoDb: {Comment}", comment.ToJson());

        var predictionResponse = _predictionService.Predict(new PredictionRequest { Text = comment.Comment } );

        if (predictionResponse.PredictedClass == 1)
        {
            _metricsService.IncrementRealTimePositiveComments();
        }
        else
        {
            _metricsService.IncrementRealTimeNegativeComments();
        }

        //_logger.LogInformation("Real time comment from MongoDb: {Comment} was labeled as {Result}", comment.Comment, predictionResponse.PredictedClass == 1 ? "Positive" : "Negative");

        await Task.CompletedTask;
    }
}
