using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using Quartz;

namespace QBitHelper.Jobs;

public class RemoveOrphanedJob(
    IOptionsMonitor<AppConfig> optionsAccessor,
    ILogger<RemoveOrphanedJob> logger,
    PathMappingService pathMappingService
) : IJob
{
    public static readonly JobKey JobKey = new("RemoveOrphanedJob");
    private readonly IOptionsMonitor<AppConfig> _optionsAccessor = optionsAccessor;
    private readonly ILogger<RemoveOrphanedJob> _logger = logger;
    private readonly PathMappingService _pathMappingService = pathMappingService;

    public Task Execute(IJobExecutionContext context)
    {
        var settings = _optionsAccessor.CurrentValue.JobConfig.Orphan;
        var orphanPathed = _pathMappingService.MapToLocalPath(settings.OrphanPath);
        var orphanedFiles = Directory.GetFiles(orphanPathed, "*", SearchOption.AllDirectories);
        _logger.LogInformation(
            "Checking {count} files in {orphanPathed} if they are older than {days} days",
            orphanedFiles.Length,
            orphanPathed,
            settings.EmptyOrphanedDirectoryAfterDays
        );

        foreach (var file in orphanedFiles)
        {
            // file is older than x days
            var fileisOlderThanXDays =
                File.GetLastWriteTimeUtc(file)
                < DateTime.UtcNow.AddDays(settings.EmptyOrphanedDirectoryAfterDays * -1);
            if (!fileisOlderThanXDays)
            {
                continue;
            }
            if (_optionsAccessor.CurrentValue.DryRun)
            {
                _logger.LogInformation(
                    "Would delete {file}",
                    _pathMappingService.MapToRemotePath(file)
                );
                continue;
            }
            _logger.LogInformation("Deleting {file}", _pathMappingService.MapToRemotePath(file));
            if (Directory.Exists(file))
                Directory.Delete(file, true);
            else if (File.Exists(file))
                File.Delete(file);
        }

        //delete empty directories
        foreach (var directory in Directory.GetDirectories(orphanPathed, "*", SearchOption.AllDirectories))
        {
            if (Directory.GetFiles(directory).Length == 0)
            {
                if (_optionsAccessor.CurrentValue.DryRun)
                {
                    _logger.LogInformation(
                        "Would delete {directory}",
                        _pathMappingService.MapToRemotePath(directory)
                    );
                    continue;
                }
                _logger.LogInformation("Deleting {directory}", _pathMappingService.MapToRemotePath(directory));
                Directory.Delete(directory);
            }
        }

        return Task.CompletedTask;
    }
}
