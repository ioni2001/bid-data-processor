using MongoDB.Bson;

namespace Big.Data.DataProcessor.Services;

public interface IRealTimeProcessorService
{
    Task StartProcessingAsync();

    Task ProcessChangeAsync(BsonDocument bsonDocument);
}
