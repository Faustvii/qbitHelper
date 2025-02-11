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
    public PreferenceConfig Preferences { get; set; } = new PreferenceConfig();

    public record PreferenceConfig
    {
        public int ListenPort { get; set; } = 6881;
    }
}

public class JobConfig
{
    public required OrphanJobConfig Orphan { get; set; } = OrphanJobConfig.Default();
    public required StalledArrJobConfig StalledArr { get; set; } = StalledArrJobConfig.Default();
    public required TagTorrentPrivacyConfig TagTorrentPrivacy { get; set; } =
        TagTorrentPrivacyConfig.Default();
    public required LimitPublicTorrentSpeedJobConfig LimitPublicTorrentSpeed { get; set; } =
        LimitPublicTorrentSpeedJobConfig.Default();
    public required ReannounceRacingTorrentConfig ReannounceRacingTorrent { get; set; } =
        ReannounceRacingTorrentConfig.Default();
    public required TagIssueTorrentConfig TagIssueTorrent { get; set; } =
        TagIssueTorrentConfig.Default();

    public required EnsureQbitPreferencesJobConfig EnsureQbitPreferences { get; set; } =
        EnsureQbitPreferencesJobConfig.Default();
}

public class LimitPublicTorrentSpeedJobConfig
{
    public required bool Enabled { get; set; }
    public required int IntervalSeconds { get; set; }
    public int MaximumUploadSpeed { get; set; } = -1;
    public int MaximumDownloadSpeed { get; set; } = -1;

    public static LimitPublicTorrentSpeedJobConfig Default()
    {
        return new LimitPublicTorrentSpeedJobConfig
        {
            Enabled = false,
            IntervalSeconds = 60,
            MaximumUploadSpeed = -1,
            MaximumDownloadSpeed = -1
        };
    }
}

public class EnsureQbitPreferencesJobConfig
{
    public required bool Enabled { get; set; }
    public required int IntervalSeconds { get; set; }

    public static EnsureQbitPreferencesJobConfig Default()
    {
        return new EnsureQbitPreferencesJobConfig { Enabled = false, IntervalSeconds = 60 };
    }
}

public class OrphanJobConfig
{
    public required bool Enabled { get; set; }
    public required string OrphanPath { get; set; }
    public required int EmptyOrphanedDirectoryAfterDays { get; set; }
    public required int IntervalMinutes { get; set; }
    public required bool ExcludeActiveTorrentRootFolders { get; set; }
    public string[] ExcludePatterns { get; set; } = [];

    public static OrphanJobConfig Default()
    {
        return new OrphanJobConfig
        {
            Enabled = false,
            ExcludeActiveTorrentRootFolders = false,
            OrphanPath = "/path/to/orphaned/files",
            EmptyOrphanedDirectoryAfterDays = 30,
            IntervalMinutes = 60,
            ExcludePatterns = []
        };
    }
}

public class StalledArrJobConfig
{
    public required bool Enabled { get; set; }
    public required int IntervalMinutes { get; set; }
    public required int MinimumTorrentAgeMinutes { get; set; }
    public required int MinimumTorrentAgeMetadataMinutes { get; set; }

    public static StalledArrJobConfig Default()
    {
        return new StalledArrJobConfig
        {
            Enabled = false,
            IntervalMinutes = 60,
            MinimumTorrentAgeMinutes = 60,
            MinimumTorrentAgeMetadataMinutes = 60
        };
    }
}

public class TagTorrentPrivacyConfig
{
    public required bool Enabled { get; set; }
    public required string PrivateTag { get; set; }
    public required string PublicTag { get; set; }
    public required int IntervalSeconds { get; set; }

    public static TagTorrentPrivacyConfig Default()
    {
        return new TagTorrentPrivacyConfig
        {
            Enabled = false,
            PrivateTag = "private",
            PublicTag = "public",
            IntervalSeconds = 60
        };
    }
}

public class ReannounceRacingTorrentConfig
{
    public required bool Enabled { get; set; }
    public required int IntervalSeconds { get; set; }
    public required int MaximumTorrentAgeSeconds { get; set; }

    public static ReannounceRacingTorrentConfig Default()
    {
        return new ReannounceRacingTorrentConfig
        {
            Enabled = false,
            IntervalSeconds = 5,
            MaximumTorrentAgeSeconds = 300
        };
    }
}

public class TagIssueTorrentConfig
{
    public required bool Enabled { get; set; }
    public required string IssueTag { get; set; }
    public required int IntervalSeconds { get; set; }

    public static TagIssueTorrentConfig Default()
    {
        return new TagIssueTorrentConfig
        {
            Enabled = false,
            IssueTag = "issue",
            IntervalSeconds = 300
        };
    }
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
