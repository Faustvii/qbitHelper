using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs
{
    public class TagIssueTorrentsJob(
        QBittorentClientAccessor qBittorentClientAccessor,
        IOptionsMonitor<AppConfig> optionsAccessor,
        ILogger<TagIssueTorrentsJob> logger,
        TimeProvider timeProvider
    ) : IJob
    {
        public static readonly JobKey JobKey = new("TagIssueTorrentsJob");
        private static readonly HashSet<string> WordsArray =
        [
            "unregistered",
            "not registered",
            "not found",
            "not exist"
        ];

        public async Task Execute(IJobExecutionContext context)
        {
            var client = await qBittorentClientAccessor.GetClient();
            var torrents = await client.GetTorrentListAsync();
            var dryRun = optionsAccessor.CurrentValue.DryRun;
            var olderThan = timeProvider.GetUtcNow().AddMinutes(-5).DateTime;
            var torrentHashesToTag = new List<string>();
            var torrentHashesToRemoveTagFrom = new List<string>();

            foreach (var torrent in torrents.Where(x => x.AddedOn < olderThan))
            {
                var hasIssues = await HasIssues(client, torrent);
                if (hasIssues)
                {
                    torrentHashesToTag.Add(torrent.Hash);
                    continue;
                }
                if (torrent.Tags.Contains("issue", StringComparer.OrdinalIgnoreCase))
                {
                    torrentHashesToRemoveTagFrom.Add(torrent.Hash);
                }
            }

            if (torrentHashesToTag.Count > 0)
            {
                logger.LogInformation(
                    "Tagging {count} torrents with 'issue' tag",
                    torrentHashesToTag.Count
                );
                if (!dryRun)
                    await client.AddTorrentTagAsync(torrentHashesToTag, "issue");
            }

            if (torrentHashesToRemoveTagFrom.Count > 0)
            {
                logger.LogInformation(
                    "Removing 'issue' tag from {count} torrents",
                    torrentHashesToRemoveTagFrom.Count
                );
                if (!dryRun)
                    await client.DeleteTorrentTagAsync(torrentHashesToRemoveTagFrom, "issue");
            }
        }

        private static async Task<bool> HasIssues(QBittorrentClient client, TorrentInfo torrent)
        {
            var trackers = await client.GetTorrentTrackersAsync(torrent.Hash);
            var activeTrackers = trackers.Where(x =>
                x.TrackerStatus != TorrentTrackerStatus.Disabled
            );
            var invalidTrackers = activeTrackers.Where(x => !IsTrackerStatusOk(x));
            return invalidTrackers.Any();
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
