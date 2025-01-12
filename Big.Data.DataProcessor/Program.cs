using AutoMapper;
using Big.Data.DataProcessor.Models.Configuration;
using Big.Data.DataProcessor.Repositories.HadoopRepositories;
using Big.Data.DataProcessor.Repositories.MongoDbRepositories;
using Big.Data.DataProcessor.Services;
using Big.Data.DataProcessor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

public class Program
{
    private static ILogger<Program>? _logger;

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
                services.AddSingleton<ICommentsHadoopRepository, CommentsHadoopRepository>();
                services.AddSingleton<ICommentsMongoDbRepository, CommentsMongoDbRepository>();
                services.AddSingleton<IBatchProcessorService, BatchProcessorService>();
                services.AddSingleton<IRealTimeProcessorService, RealTimeProcessorService>();

                //services.AddHostedService<CommentsBatchProcessor>();
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

        await appHost.RunAsync();
    }
}