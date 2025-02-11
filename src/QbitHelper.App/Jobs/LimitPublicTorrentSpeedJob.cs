using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs;

public class LimitPublicTorrentSpeedJob(
    IOptionsMonitor<AppConfig> optionsAccessor,
    QBittorentClientAccessor qBittorentClientAccessor,
    ILogger<LimitPublicTorrentSpeedJob> logger
) : IJob
{
    public static readonly JobKey JobKey = new("LimitPublicTorrentSpeedJob");

    public async Task Execute(IJobExecutionContext context)
    {
        var client = await qBittorentClientAccessor.GetClient();
        var config = optionsAccessor.CurrentValue;
        var publicTag = config.JobConfig.TagTorrentPrivacy.PublicTag;
        var publicTorrents = await client.GetTorrentListAsync(
            query: new TorrentListQuery() { Tag = publicTag }
        );
        var limitConfig = config.JobConfig.LimitPublicTorrentSpeed;
        var hasDownloadLimit = limitConfig.MaximumDownloadSpeed > 0;
        var hasUploadLimit = limitConfig.MaximumUploadSpeed > 0;
        var downloadLimitTorrents = publicTorrents.Where(x =>
            hasDownloadLimit && x.DownloadLimit != limitConfig.MaximumDownloadSpeed
        );
        var uploadLimitTorrents = publicTorrents.Where(x =>
            hasUploadLimit && x.UploadLimit != limitConfig.MaximumUploadSpeed
        );

        if (!downloadLimitTorrents.Any() && !uploadLimitTorrents.Any())
        {
            logger.LogDebug("No public torrents to limit");
            return;
        }

        IEnumerable<TorrentInfo> allTorrents = [.. downloadLimitTorrents, .. uploadLimitTorrents];

        logger.LogDebug(
            "Found {torrentCount} public torrents without configured limits",
            allTorrents.DistinctBy(x => x.Hash).Count()
        );

        if (downloadLimitTorrents.Any() && !config.DryRun)
        {
            logger.LogInformation(
                "Setting download limits for {count} public torrents to {maxSpeed}",
                downloadLimitTorrents.Count(),
                limitConfig.MaximumDownloadSpeed
            );
            await client.SetTorrentDownloadLimitAsync(
                downloadLimitTorrents.Select(x => x.Hash),
                limitConfig.MaximumDownloadSpeed,
                context.CancellationToken
            );
        }
        if (uploadLimitTorrents.Any() && !config.DryRun)
        {
            logger.LogInformation(
                "Setting upload limits for {count} public torrents to {maxSpeed}",
                uploadLimitTorrents.Count(),
                limitConfig.MaximumUploadSpeed
            );
            await client.SetTorrentUploadLimitAsync(
                uploadLimitTorrents.Select(x => x.Hash),
                limitConfig.MaximumUploadSpeed,
                context.CancellationToken
            );
        }
    }
}
