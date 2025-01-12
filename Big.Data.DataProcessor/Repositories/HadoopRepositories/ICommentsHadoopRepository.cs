using Big.Data.DataProcessor.Models.DbEntities;

namespace Big.Data.DataProcessor.Repositories.HadoopRepositories;

public interface ICommentsHadoopRepository
{
    Task<List<SocialMediaComment>> GetCommentsAsync(int batchNumber, int batchSize);
}
