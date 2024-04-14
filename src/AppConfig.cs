namespace QBitHelper;

public class AppConfig
{
    public const string ConfigurationSectionName = "Settings";

    public required bool DryRun { get; set; }
    public required JobConfig JobConfig { get; set; }
    public required QbittorrentConfig QbittorrentConfig { get; set; }
    public PathMapping[] PathMappings { get; set; } = [];
    public Dictionary<string, ArrConfig> TorrentCategoryArrConfigs { get; set; } = [];
}

public class QbittorrentConfig
{
    public required string Host { get; set; }
    public required string DownloadPath { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class JobConfig
{
    public required OrphanJobConfig Orphan { get; set; }
    public required StalledArrJobConfig StalledArr { get; set; }
    public required TagTorrentPrivacyConfig TagTorrentPrivacy { get; set; }
}

public class OrphanJobConfig
{
    public required bool Enabled { get; set; }
    public required string OrphanPath { get; set; }
    public required int EmptyOrphanedDirectoryAfterDays { get; set; }
    public required int IntervalMinutes { get; set; }
    public string[] ExcludePatterns { get; set; } = [];
}

public class StalledArrJobConfig
{
    public required bool Enabled { get; set; }
    public required int IntervalMinutes { get; set; }
    public required int MinimumTorrentAgeMinutes { get; set; }
    public required int MinimumTorrentAgeMetadataMinutes { get; set; }
}

public class TagTorrentPrivacyConfig
{
    public required bool Enabled { get; set; }
    public required string PrivateTag { get; set; }
    public required string PublicTag { get; set; }
    public required int IntervalMinutes { get; set; }
}

public class PathMapping
{
    public required string LocalPath { get; set; }
    public required string RemotePath { get; set; }
}

public class ArrConfig
{
    public required ArrType Type { get; set; }
    public required string Name { get; set; }
    public required string Host { get; set; }
    public required string ApiKey { get; set; }
}

public enum ArrType
{
    Radarr,
    Sonarr,
    Readarr
}
