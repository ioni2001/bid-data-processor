namespace Big.Data.DataProcessor.Services;

public interface IBatchProcessorService
{
    Task ProcessCommentsStreamAsync(int batchSize);
}
