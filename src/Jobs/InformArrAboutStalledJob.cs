using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBitHelper.Services.Dtos;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs;

public class InformArrAboutStalledJob(
    TimeProvider timeProvider,
    QBittorentClientAccessor qBittorentClientAccessor,
    ILogger<InformArrAboutStalledJob> logger,
    IOptionsMonitor<AppConfig> optionsAccessor,
    ArrClient arrClient
) : IJob
{
    public static readonly JobKey JobKey = new("InformArrAboutStalledJob");

    public async Task Execute(IJobExecutionContext context)
    {
        var now = timeProvider.GetUtcNow();
        var settings = optionsAccessor.CurrentValue;
        var client = await qBittorentClientAccessor.GetClient();
        var torrents = await client.GetTorrentListAsync();
        var limitDate = now.AddMinutes(settings.JobConfig.StalledArr.MinimumTorrentAgeMinutes * -1);
        var metadatalimitDate = now.AddMinutes(
            settings.JobConfig.StalledArr.MinimumTorrentAgeMetadataMinutes * -1
        );
        var stalledBoys = torrents.Where(x =>
            x.State == TorrentState.StalledDownload && x.AddedOn < limitDate
        );
        var stalledMetadataBoys = torrents.Where(x =>
            x.State == TorrentState.FetchingMetadata && x.AddedOn < metadatalimitDate
        );

        if (!stalledBoys.Any() && !stalledMetadataBoys.Any())
        {
            logger.LogDebug("No stalled torrents found");
            return;
        }

        var arrCategories = settings.TorrentCategoryArrConfigs.Keys;
        var arrQueue = new Dictionary<string, IEnumerable<QueueRecord>>();
        foreach (var category in arrCategories)
        {
            arrQueue[category] = await arrClient.GetQueue(category);
        }

        foreach (var torrent in stalledMetadataBoys)
        {
            await InformArrAboutStalled(
                logger,
                arrClient,
                now,
                settings,
                client,
                arrQueue,
                torrent
            );
        }

        foreach (var torrent in stalledBoys)
        {
            await InformArrAboutStalled(
                logger,
                arrClient,
                now,
                settings,
                client,
                arrQueue,
                torrent
            );
        }
    }

    private static async Task InformArrAboutStalled(
        ILogger<InformArrAboutStalledJob> logger,
        ArrClient arrClient,
        DateTimeOffset now,
        AppConfig settings,
        QBittorrentClient client,
        Dictionary<string, IEnumerable<QueueRecord>> arrQueue,
        TorrentInfo torrent
    )
    {
        var torrentProps = await client.GetTorrentPropertiesAsync(torrent.Hash);
        var torrentIsPrivate = true;
        if (torrentProps.AdditionalData.TryGetValue("is_private", out var isPrivate))
        {
            torrentIsPrivate = isPrivate?.ToObject<bool>() ?? true;
        }

        var age = now - (torrent.AddedOn ?? now);
        if (!arrQueue.TryGetValue(torrent.Category, out var queue))
            return;

        var torrentQueue = queue.FirstOrDefault(x =>
            x.DownloadId.Equals(torrent.Hash, StringComparison.OrdinalIgnoreCase)
        );
        if (
            torrentQueue is null
            || !torrentQueue.Status.Equals("Warning", StringComparison.OrdinalIgnoreCase)
            && !torrentQueue.Status.Equals("Queued", StringComparison.OrdinalIgnoreCase)
        )
            return;

        if (settings.DryRun)
        {
            logger.LogInformation(
                "{torrentName} is stalled and has been in queue for {torrentAge:F2} days - Would inform arr to blacklist and search for new release",
                torrent.Name,
                age.TotalDays
            );
            return;
        }

        logger.LogInformation(
            "{torrentName} is stalled and has been in queue for {torrentAge:F2} days - informing arr to blacklist and search for new release",
            torrent.Name,
            age.TotalDays
        );
        await arrClient.RemoveFromQueue(
            torrent.Category,
            torrentQueue.Id,
            removeFromClient: !torrentIsPrivate
        );
    }
}
