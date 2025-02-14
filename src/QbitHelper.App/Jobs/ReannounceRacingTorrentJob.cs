using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs
{
    public class ReannounceRacingTorrentJob(
        QBittorentClientAccessor qBittorentClientAccessor,
        TimeProvider timeProvider,
        IOptionsMonitor<AppConfig> optionsAccessor,
        ILogger<ReannounceRacingTorrentJob> logger
    ) : IJob
    {
        public static readonly JobKey JobKey = new("ReannounceRacingTorrentJob");
        private static readonly HashSet<string> WordsArray =
        [
            "unregistered",
            "not registered",
            "not found",
            "not exist",
        ];

        public async Task Execute(IJobExecutionContext context)
        {
            var jobConfig = optionsAccessor.CurrentValue.JobConfig.ReannounceRacingTorrent;
            var dryRun = optionsAccessor.CurrentValue.DryRun;
            var client = await qBittorentClientAccessor.GetClient();
            var latestTorrents = await client.GetTorrentListAsync(
                new TorrentListQuery
                {
                    SortBy = "added_on",
                    ReverseSort = true,
                    Limit = 10,
                }
            );

            var now = timeProvider
                .GetUtcNow()
                .AddSeconds(jobConfig.MaximumTorrentAgeSeconds * -1)
                .DateTime;
            foreach (var torrent in latestTorrents.Where(x => x.AddedOn > now))
            {
                if (torrent.State != TorrentState.StalledDownload)
                    continue;

                var trackers = await client.GetTorrentTrackersAsync(torrent.Hash);
                var activeTrackers = trackers.Where(x =>
                    x.TrackerStatus != TorrentTrackerStatus.Disabled
                );
                var invalidTrackers = activeTrackers.Where(x => !IsTrackerStatusOk(x));
                if (!invalidTrackers.Any())
                    continue;

                logger.LogInformation(
                    "Reannouncing torrent {TorrentName} with hash {TorrentHash}",
                    torrent.Name,
                    torrent.Hash
                );

                if (!dryRun)
                    await client.ReannounceAsync(torrent.Hash, context.CancellationToken);
            }
        }

        private static bool IsTrackerStatusOk(TorrentTracker tracker)
        {
            if (
                WordsArray.Any(x => tracker.Message.Contains(x, StringComparison.OrdinalIgnoreCase))
            )
                return false;

            return tracker.TrackerStatus == TorrentTrackerStatus.Working;
        }
    }
}
