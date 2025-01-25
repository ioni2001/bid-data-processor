using Big.Data.DataProcessor.Services;

namespace Big.Data.DataProcessor;

public class ConsoleDashboard
{
    private readonly MetricsService _metricsService;
    private Timer _timer;

    public ConsoleDashboard(MetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    public void Start()
    {
        _timer = new Timer(UpdateDashboard, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.4));
    }

    private void UpdateDashboard(object state)
    {
        Console.Clear();
        Console.WriteLine("Real-Time Processing Metrics Dashboard");
        Console.WriteLine("===========================");
        Console.WriteLine($"Processed Comments: {_metricsService.GetRealTimeProcessedComments()}");
        Console.WriteLine($"Positive Comments: {_metricsService.GetRealTimePositiveComments()}");
        Console.WriteLine($"Negative Comments: {_metricsService.GetRealTimeNegativeComments()}");
        Console.WriteLine("\n");
        Console.WriteLine("\n");
        Console.WriteLine("\n");
        Console.WriteLine("\n");
        Console.WriteLine("Batch Processing Metrics Dashboard");
        Console.WriteLine("===========================");
        Console.WriteLine($"Processed Comments: {_metricsService.GetBatchProcessedComments()}");
        Console.WriteLine($"Positive Comments: {_metricsService.GetBatchPositiveComments()}");
        Console.WriteLine($"Negative Comments: {_metricsService.GetBatchNegativeComments()}");
    }
}
