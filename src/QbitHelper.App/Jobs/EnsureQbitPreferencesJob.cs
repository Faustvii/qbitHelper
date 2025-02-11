using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using Quartz;

namespace QBitHelper.Jobs
{
    public class EnsureQbitPreferencesJob(
        QBittorentClientAccessor qBittorentClientAccessor,
        IOptionsMonitor<AppConfig> optionsAccessor,
        ILogger<EnsureQbitPreferencesJob> logger
    ) : IJob
    {
        public static readonly JobKey JobKey = new("EnsureQbitPreferencesJob");

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = optionsAccessor.CurrentValue.QbittorrentConfig.Preferences;
            var client = await qBittorentClientAccessor.GetClient();
            var preferences = await client.GetPreferencesAsync();
            var dryRun = optionsAccessor.CurrentValue.DryRun;

            if (preferences.ListenPort != settings.ListenPort)
            {
                logger.LogInformation(
                    "Updating listen port to {port} from {currentPort}",
                    settings.ListenPort,
                    preferences.ListenPort
                );

                if (!dryRun)
                {
                    preferences.ListenPort = settings.ListenPort;
                    await client.SetPreferencesAsync(preferences);
                }
            }
        }
    }
}
