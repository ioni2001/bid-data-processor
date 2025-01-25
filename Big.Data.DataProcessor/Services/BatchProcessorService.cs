using Big.Data.DataProcessor.Models.DbEntities;
using Big.Data.DataProcessor.Repositories.HadoopRepositories;
using Microsoft.Extensions.Logging;

namespace Big.Data.DataProcessor.Services;

public class BatchProcessorService : IBatchProcessorService
{
    private readonly ILogger<BatchProcessorService> _logger;
    private readonly ICommentsHadoopRepository _commentsHadoopRepository;
    private readonly IPredictionService _predictionService;
    private readonly MetricsService _metricsService;

    public BatchProcessorService(
        ILogger<BatchProcessorService> logger,
        ICommentsHadoopRepository commentsHadoopRepository,
        IPredictionService predictionService,
        MetricsService metricsService)
    {
        _logger = logger;
        _commentsHadoopRepository = commentsHadoopRepository;
        _predictionService = predictionService;
        _metricsService = metricsService;
    }

    public async Task ProcessCommentsStreamAsync(int batchSize)
    {
        int processedCount = 0;
        int batchNumber = 1;

        while (true)
        {
            var batch = await _commentsHadoopRepository.GetCommentsAsync(batchNumber, batchSize);

            if (batch == null || batch.Count == 0)
            {
                break;
            }

            //_logger.LogInformation("Received batch number {BatchNumber} of size {BatchSize} from HDFS", batchNumber, batchSize);

            await ProcessCommentBatchAsync(batch);
            processedCount += batch.Count;

            //_logger.LogInformation("Processed batch number {BatchNumber} of size {BatchSize} from HDFS. Total processed: {TotalProcessed}", batchNumber, batchSize, processedCount);

            batchNumber++;
        }
    }

    private async Task ProcessCommentBatchAsync(List<SocialMediaComment> comments)
    {
        foreach (var comment in comments)
        {
            await ProcessSingleCommentAsync(comment);
            _metricsService.IncrementBatchProcessedComments();
        }
    }

    private async Task ProcessSingleCommentAsync(SocialMediaComment comment)
    {
        var predictionResponse = _predictionService.Predict(new PredictionRequest { Text = comment.Comment });

        if (predictionResponse.PredictedClass == 1)
        {
            _metricsService.IncrementBatchPositiveComments();
        }
        else
        {
            _metricsService.IncrementBatchNegativeComments();
        }

        //_logger.LogInformation("Batch comment from HDFS: {Comment} was labeled as {Result}", comment.Comment, predictionResponse.PredictedClass == 1 ? "Positive" : "Negative");

        await Task.CompletedTask;
    }
}
