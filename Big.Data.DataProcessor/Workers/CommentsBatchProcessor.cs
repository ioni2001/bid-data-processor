using Big.Data.DataProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Big.Data.DataProcessor.Workers;

public class CommentsBatchProcessor : BackgroundService
{
    private readonly IBatchProcessorService _batchProcessorService;
    private readonly ILogger<CommentsBatchProcessor> _logger;
    private const int DefaultBatchSize = 100;

    public CommentsBatchProcessor(IBatchProcessorService batchProcessorService, ILogger<CommentsBatchProcessor> logger)
    {
        _logger = logger;
        _batchProcessorService = batchProcessorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogInformation("Started comments batch processing from HDFS...");

        await _batchProcessorService.ProcessCommentsStreamAsync(DefaultBatchSize);

        //_logger.LogInformation("Finished comments batch processing from HDFS...");

        await Task.CompletedTask;
    }
}
