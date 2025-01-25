using System.Diagnostics.Metrics;

namespace Big.Data.DataProcessor.Services;

public class MetricsService
{
    private readonly Counter<long> _realTimeProcessedCommentsCounter;
    private readonly Counter<long> _realTimePositiveCommentsCounter;
    private readonly Counter<long> _realTimeNegativeCommentsCounter;

    private readonly Counter<long> _batchProcessedCommentsCounter;
    private readonly Counter<long> _batchPositiveCommentsCounter;
    private readonly Counter<long> _batchNegativeCommentsCounter;

    private long _realTimeProcessedComments;
    private long _realTimePositiveComments;
    private long _realTimeNegativeComments;

    private long _batchProcessedComments;
    private long _batchPositiveComments;
    private long _batchNegativeComments;

    public MetricsService()
    {
        var meter = new Meter("Big-Data-Processing", "1.0.0");

        // Real-time processing metrics
        _realTimeProcessedCommentsCounter = meter.CreateCounter<long>("realtime_processed_comments");
        _realTimePositiveCommentsCounter = meter.CreateCounter<long>("realtime_positive_comments");
        _realTimeNegativeCommentsCounter = meter.CreateCounter<long>("realtime_negative_comments");

        // Batch processing metrics
        _batchProcessedCommentsCounter = meter.CreateCounter<long>("batch_processed_comments");
        _batchPositiveCommentsCounter = meter.CreateCounter<long>("batch_positive_comments");
        _batchNegativeCommentsCounter = meter.CreateCounter<long>("batch_negative_comments");
    }

    // Real-time processing increment methods
    public void IncrementRealTimeProcessedComments()
    {
        Interlocked.Increment(ref _realTimeProcessedComments);
        _realTimeProcessedCommentsCounter.Add(1);
    }

    public void IncrementRealTimePositiveComments()
    {
        Interlocked.Increment(ref _realTimePositiveComments);
        _realTimePositiveCommentsCounter.Add(1);
    }

    public void IncrementRealTimeNegativeComments()
    {
        Interlocked.Increment(ref _realTimeNegativeComments);
        _realTimeNegativeCommentsCounter.Add(1);
    }

    // Batch processing increment methods
    public void IncrementBatchProcessedComments()
    {
        Interlocked.Increment(ref _batchProcessedComments);
        _batchProcessedCommentsCounter.Add(1);
    }

    public void IncrementBatchPositiveComments()
    {
        Interlocked.Increment(ref _batchPositiveComments);
        _batchPositiveCommentsCounter.Add(1);
    }

    public void IncrementBatchNegativeComments()
    {
        Interlocked.Increment(ref _batchNegativeComments);
        _batchNegativeCommentsCounter.Add(1);
    }

    // Real-time processing getters
    public long GetRealTimeProcessedComments() => _realTimeProcessedComments;
    public long GetRealTimePositiveComments() => _realTimePositiveComments;
    public long GetRealTimeNegativeComments() => _realTimeNegativeComments;

    // Batch processing getters
    public long GetBatchProcessedComments() => _batchProcessedComments;
    public long GetBatchPositiveComments() => _batchPositiveComments;
    public long GetBatchNegativeComments() => _batchNegativeComments;
}
