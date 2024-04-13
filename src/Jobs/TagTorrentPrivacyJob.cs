using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs;

public class TagTorrentPrivacyJob(
    IOptionsMonitor<SettingsOptions> optionsAccessor,
    ILogger<TagTorrentPrivacyJob> logger,
    QBittorentClientAccessor qBittorentClientAccessor
) : IJob
{
    public static readonly JobKey JobKey = new("TagTorrentPrivacy");

    public async Task Execute(IJobExecutionContext context)
    {
        var settings = optionsAccessor.CurrentValue;
        var tagTorrentSettings = settings.JobConfig.TagTorrentPrivacy;
        var client = await qBittorentClientAccessor.GetClient();
        var torrents = await client.GetTorrentListAsync();
        var tags = await client.GetTagsAsync();
        var tagsToCreate = new List<string>();
        if (!tags.Contains(tagTorrentSettings.PublicTag, StringComparer.OrdinalIgnoreCase))
            tagsToCreate.Add(tagTorrentSettings.PublicTag);
        if (!tags.Contains(tagTorrentSettings.PrivateTag, StringComparer.OrdinalIgnoreCase))
            tagsToCreate.Add(tagTorrentSettings.PrivateTag);
        if (tagsToCreate.Count > 0 && !settings.DryRun)
            await client.CreateTagsAsync(tagsToCreate);

        var torrentsToTag = torrents
            .Where(x =>
                !x.Tags.Contains(tagTorrentSettings.PublicTag, StringComparer.OrdinalIgnoreCase)
                && !x.Tags.Contains(tagTorrentSettings.PrivateTag, StringComparer.OrdinalIgnoreCase)
            )
            .ToList();

        if (torrentsToTag.Count == 0)
        {
            logger.LogDebug("All torrents are already tagged");
            return;
        }

        logger.LogInformation("We have {count} torrents to tag", torrentsToTag.Count);

        foreach (var torrent in torrentsToTag)
        {
            var torrentProps = await client.GetTorrentPropertiesAsync(torrent.Hash);
            var torrentIsPrivate = true;
            if (torrentProps.AdditionalData.TryGetValue("is_private", out var isPrivate))
            {
                torrentIsPrivate = isPrivate?.ToObject<bool>() ?? true;
            }
            if (settings.DryRun)
            {
                logger.LogInformation(
                    "Would tag torrent {name} as {tag}",
                    torrent.Name,
                    torrentIsPrivate ? tagTorrentSettings.PrivateTag : tagTorrentSettings.PublicTag
                );
                continue;
            }
            if (torrentIsPrivate)
                await client.AddTorrentTagAsync(torrent.Hash, tagTorrentSettings.PrivateTag);
            else
                await client.AddTorrentTagAsync(torrent.Hash, tagTorrentSettings.PublicTag);
        }

        logger.LogInformation("Tagged {count} torrents", torrentsToTag.Count);
    }
}
