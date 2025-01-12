using MongoDB.Bson;

namespace Big.Data.DataProcessor.Repositories.MongoDbRepositories;

public interface ICommentsMongoDbRepository
{
    Task WatchCommentsAsync(Func<BsonDocument, Task> processChange);
}
