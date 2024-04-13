using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs;

public partial class MoveOrphanedJob(
    QBittorentClientAccessor qBittorentClientAccessor,
    PathMappingService pathMappingService,
    IOptionsMonitor<SettingsOptions> optionsAccessor,
    ILogger<MoveOrphanedJob> logger
) : IJob
{
    public static readonly JobKey JobKey = new("MoveOrphanedJob");
    private readonly QBittorentClientAccessor _qBittorentClientAccessor = qBittorentClientAccessor;
    private readonly PathMappingService _pathMappingService = pathMappingService;
    private readonly IOptionsMonitor<SettingsOptions> _optionsAccessor = optionsAccessor;
    private readonly ILogger<MoveOrphanedJob> _logger = logger;
    private static readonly Regex _extensionRegex = FileExtensionRegex();

    public async Task Execute(IJobExecutionContext context)
    {
        var client = await _qBittorentClientAccessor.GetClient();
        var clientSettings = await client.GetPreferencesAsync();
        var torrents = await client.GetTorrentListAsync();
        var allTorrentFiles = await GetTorrentFiles(torrents, client, clientSettings);
        var localPaths = allTorrentFiles.Select(_pathMappingService.MapToLocalPath).ToList();
        var settings = _optionsAccessor.CurrentValue;
        var qbitSettings = settings.QbittorrentConfig;
        var localPath = _pathMappingService.MapToLocalPath(qbitSettings.DownloadPath);
        var localOrphanPath = _pathMappingService.MapToLocalPath(
            settings.JobConfig.Orphan.OrphanPath
        );
        var globPatterns = settings.JobConfig.Orphan.ExcludePatterns;
        var matcher = new Matcher();
        matcher.AddIncludePatterns(globPatterns);

        var allFiles = Directory
            .GetFiles(localPath, "*", SearchOption.AllDirectories)
            .Where(file => !file.Contains(localOrphanPath, StringComparison.Ordinal))
            .Where(file => !matcher.Match(file).HasMatches)
            .ToArray();

        var localOrphanedFiles = allFiles.Except(localPaths);
        _logger.LogInformation(
            "Found orphaned files: {count} out of {allCount}",
            localOrphanedFiles.Count(),
            allFiles.Length
        );

        foreach (var filePath in localOrphanedFiles)
        {
            var mappedPath = filePath.Replace(localPath, "");
            _logger.LogDebug(
                "{filePath} to {destinationPath}",
                _pathMappingService.MapToRemotePath(filePath),
                _pathMappingService.MapToRemotePath(Path.Combine(localOrphanPath, mappedPath))
            );
            if (settings.DryRun)
                continue;

            Directory.Move(filePath, Path.Combine(localOrphanPath, mappedPath));
        }
        _logger.LogInformation("Moved orphaned {count} files", localOrphanedFiles.Count());
    }

    private static async Task<string[]> GetTorrentFiles(
        IReadOnlyList<TorrentInfo> torrents,
        QBittorrentClient client,
        Preferences clientSettings
    )
    {
        var allFilePaths = new List<string>();

        foreach (var torrent in torrents)
        {
            var singleFileTorrent = _extensionRegex.IsMatch(torrent.ContentPath);
            if (singleFileTorrent)
            {
                allFilePaths.Add(
                    GetFilePathForSingleFileTorrent(
                        clientSettings.AppendExtensionToIncompleteFiles ?? false,
                        torrent
                    )
                );
                continue;
            }
            allFilePaths.AddRange(await GetFilePathsForTorrent(client, clientSettings, torrent));
        }
        return [.. allFilePaths];
    }

    private static async Task<string[]> GetFilePathsForTorrent(
        QBittorrentClient client,
        Preferences clientSettings,
        TorrentInfo torrent
    )
    {
        var filePaths = new List<string>();
        var content = await client.GetTorrentContentsAsync(torrent.Hash);
        foreach (var file in content)
        {
            if (
                file.Progress < 1
                && clientSettings.AppendExtensionToIncompleteFiles.GetValueOrDefault()
            )
            {
                var torrentSavePath = torrent.SavePath;
                if (clientSettings.TempPathEnabled == true)
                    torrentSavePath = clientSettings.TempPath;

                filePaths.Add(Path.Combine(torrentSavePath, $"{file.Name}.!qB"));
            }
            else
            {
                filePaths.Add(Path.Combine(torrent.SavePath, file.Name));
            }
        }
        return [.. filePaths];
    }

    private static string GetFilePathForSingleFileTorrent(
        bool appendExtensionToIncompleteFiles,
        TorrentInfo torrent
    )
    {
        var path = torrent.ContentPath;
        if (torrent.Progress < 1 && appendExtensionToIncompleteFiles)
        {
            return $"{torrent.ContentPath}.!qB";
        }
        return path;
    }

    [GeneratedRegex(@"\.\w{1,5}$", RegexOptions.Compiled, matchTimeoutMilliseconds: 100)]
    private static partial Regex FileExtensionRegex();
}
