using Big.Data.DataProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Big.Data.DataProcessor.Workers;

public class CommentsRealTimeProcessor : BackgroundService
{
    private readonly IRealTimeProcessorService _realTimeProcessorService;
    private readonly ILogger<CommentsRealTimeProcessor> _logger;

    public CommentsRealTimeProcessor(IRealTimeProcessorService realTimeProcessorService, ILogger<CommentsRealTimeProcessor> logger)
    {
        _logger = logger;
        _realTimeProcessorService = realTimeProcessorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _realTimeProcessorService.StartProcessingAsync();

        await Task.CompletedTask;
    }
}
