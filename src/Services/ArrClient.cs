using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services.Dtos;

namespace QBitHelper.Services;

public sealed class ArrClient(
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<SettingsOptions> optionsAccessor,
    ILogger<ArrClient> logger
)
{
    public async Task<IEnumerable<QueueRecord>> GetQueue(string torrentCategory)
    {
        var client = httpClientFactory.CreateClient();
        var settings = optionsAccessor.CurrentValue;
        settings.TorrentCategoryArrConfigs.TryGetValue(torrentCategory, out var arrConfig);
        if (arrConfig is null)
        {
            logger.LogWarning("Could not find arr config for category {category}", torrentCategory);
            return [];
        }
        client.BaseAddress = new Uri(arrConfig.Host);
        client.DefaultRequestHeaders.Add("X-Api-Key", arrConfig.ApiKey);

        var page = 1;
        var pageSize = 10;
        var queue = await GetQueuePageResponse(client, page, pageSize);
        if (queue is null)
        {
            return [];
        }
        var records = new List<QueueRecord>(queue.Records);
        if (queue.TotalRecords != queue.Records.Length)
        {
            page++;
            while (records.Count < queue.TotalRecords)
            {
                queue = await GetQueuePageResponse(client, page, pageSize);
                if (queue is null)
                {
                    break;
                }
                records.AddRange(queue.Records);
            }
        }
        return records;
    }

    public async Task RemoveFromQueue(
        string torrentCategory,
        long queueId,
        bool removeFromClient = false,
        bool blocklist = true,
        bool skipRedownload = false,
        bool changeCategory = false
    )
    {
        var client = httpClientFactory.CreateClient();
        var settings = optionsAccessor.CurrentValue;
        settings.TorrentCategoryArrConfigs.TryGetValue(torrentCategory, out var arrConfig);
        if (arrConfig is null)
        {
            logger.LogWarning("Could not find arr config for category {category}", torrentCategory);
            return;
        }
        client.BaseAddress = new Uri(arrConfig.Host);
        client.DefaultRequestHeaders.Add("X-Api-Key", arrConfig.ApiKey);

        var response = await client.DeleteAsync(
            $"/api/v3/queue/{queueId}"
                + $"?removeFromClient={removeFromClient}"
                + $"&blocklist={blocklist}"
                + $"&skipRedownload={skipRedownload}"
                + $"&changeCategory={changeCategory}"
        );
        response.EnsureSuccessStatusCode();
    }

    private static async Task<QueueResourcePagingResource?> GetQueuePageResponse(
        HttpClient client,
        int page,
        int pageSize
    )
    {
        var response = await client.GetAsync(
            $"/api/v3/queue?page={page}&pageSize={pageSize}"
                + "&includeUnknownSeriesItems=false&includeSeries=false&includeEpisode=false&protocol=torrent"
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QueueResourcePagingResource>();
    }
}
