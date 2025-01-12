using Big.Data.DataProcessor.Models.Configuration;
using Big.Data.DataProcessor.Models.DbEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebHdfs.Client;

namespace Big.Data.DataProcessor.Repositories.HadoopRepositories;

public class CommentsHadoopRepository : ICommentsHadoopRepository
{
    private readonly WebHdfsClient _webHdfsClient;
    private readonly HadoopSettings _hadoopSettings;
    private readonly ILogger<CommentsHadoopRepository> _logger;

    public CommentsHadoopRepository(IOptions<HadoopSettings> hadoopSettings, ILogger<CommentsHadoopRepository> logger)
    {
        _hadoopSettings = hadoopSettings.Value;
        _webHdfsClient = new WebHdfsClient(_hadoopSettings.BaseAddress, _hadoopSettings.Username);
        _logger = logger;
    }

    public async Task<List<SocialMediaComment>> GetCommentsAsync(int batchNumber, int batchSize)
    {
        var files = await _webHdfsClient.ListFileStatusAsync(_hadoopSettings.CommentsDirectoryPath);
        var comments = new List<SocialMediaComment>();

        foreach (var file in files.Skip((batchNumber - 1) * batchSize).Take(batchSize))
        {
            var contentStream = await _webHdfsClient.ReadStreamAsync($"{_hadoopSettings.CommentsDirectoryPath}/{file.PathSuffix}");

            using (var reader = new StreamReader(contentStream))
            {
                var contentString = await reader.ReadToEndAsync();
                var comment = JsonConvert.DeserializeObject<SocialMediaComment>(contentString);
                comments.Add(comment!);
            }

        }

        _logger.LogInformation("Retrieved {Count} comments from HDFS", comments.Count);
        return comments;
    }
}
