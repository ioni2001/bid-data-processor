using AutoMapper;
using Big.Data.DataProcessor;
using Big.Data.DataProcessor.Models.Configuration;
using Big.Data.DataProcessor.Repositories.HadoopRepositories;
using Big.Data.DataProcessor.Repositories.MongoDbRepositories;
using Big.Data.DataProcessor.Services;
using Big.Data.DataProcessor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

public class Program
{
    private static ILogger<Program>? _logger;
    const string serviceName = "Big-Data-Processing";

    static async Task Main(string[] args)
    {
        IHost appHost = Host
            .CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
            })
            .ConfigureLogging((hbc, logging) =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;

                    options.AddConsoleExporter();
                });
            })
            .ConfigureServices((hbc, services) =>
            {
                services.AddOpenTelemetry()
                        .ConfigureResource(resource => resource.AddService(serviceName))
                        .WithMetrics(metrics => metrics
                            .AddMeter(serviceName)
                            .AddConsoleExporter());

                services.AddSingleton<IHostEnvironment>(new HostingEnvironment { EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") });
                services.AddSingleton<ICommentsHadoopRepository, CommentsHadoopRepository>();
                services.AddSingleton<ICommentsMongoDbRepository, CommentsMongoDbRepository>();
                services.AddSingleton<IBatchProcessorService, BatchProcessorService>();
                services.AddSingleton<IRealTimeProcessorService, RealTimeProcessorService>();
                services.AddSingleton<IPredictionService, PredictionService>();
                services.AddSingleton<MetricsService>();
                services.AddSingleton<ConsoleDashboard>();

                services.AddHostedService<CommentsBatchProcessor>();
                services.AddHostedService<CommentsRealTimeProcessor>();

                services.Configure<HadoopSettings>(hbc.Configuration.GetRequiredSection("HadoopSettings"));
                services.Configure<MongoDbSettings>(hbc.Configuration.GetRequiredSection("MongoDbSettings"));

                /*var mapperConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfile());
                });

                IMapper mapper = mapperConfig.CreateMapper();
                services.AddSingleton(mapper);*/
            }).Build();

        _logger = appHost.Services.GetRequiredService<ILogger<Program>>();

        _logger.LogInformation("App Host created successfully");

        var dashboard = appHost.Services.GetRequiredService<ConsoleDashboard>();
        dashboard.Start();

        await appHost.RunAsync();
    }
}