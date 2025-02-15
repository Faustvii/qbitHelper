using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Services;
using QBittorrent.Client;
using Quartz;

namespace QBitHelper.Jobs
{
    public class EnsureQbitConnectableJob(
        QBittorentClientAccessor qBittorentClientAccessor,
        IOptionsMonitor<AppConfig> optionsAccessor,
        ILogger<EnsureQbitConnectableJob> logger
    ) : IJob
    {
        public static readonly JobKey JobKey = new(nameof(EnsureQbitConnectableJob));
        private const string LastLogIdKey = "LastLogId";
        private const string LastExternalIPKey = "LastExternalIP";

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = optionsAccessor.CurrentValue.QbittorrentConfig.Preferences;
            var client = await qBittorentClientAccessor.GetClient();
            var dryRun = optionsAccessor.CurrentValue.DryRun;

            var externalIp = await ExtractExternalIP(client);
            if (externalIp is null)
                return;

            logger.LogInformation(
                "Testing Qbittorrent connection on {ip}:{port}",
                externalIp,
                settings.ListenPort
            );
            var isConnectable = await NetworkUtils.IsConnectable(
                externalIp,
                settings.ListenPort,
                TimeSpan.FromSeconds(5)
            );
            if (isConnectable)
            {
                logger.LogInformation("Qbittorrent is connectable: {isConnectable}", isConnectable);
                return;
            }

            var preferences = await client.GetPreferencesAsync();
            var nicPreference = preferences.CurrentNetworkInterface;
            var preferenceUpdate = new Preferences
            {
                CurrentNetworkInterface = nicPreference,
                CurrentInterfaceAddress =
                    preferences.CurrentInterfaceAddress == string.Empty ? "0.0.0.0" : string.Empty,
            };

            logger.LogWarning(
                "Qbittorrent is not connectable - updating network interface to {nic} with address {address}",
                preferenceUpdate.CurrentNetworkInterface,
                preferenceUpdate.CurrentInterfaceAddress
            );
            // Trigger network interface update in Qbit so it binds again
            if (dryRun)
                return;

            await client.SetPreferencesAsync(preferenceUpdate);
        }

        private async Task<string?> ExtractExternalIP(QBittorrentClient client)
        {
            var lastLogId = JobStateStore.Get<int>(LastLogIdKey);
            var allLogs = await client.GetLogAsync(TorrentLogSeverity.Info, afterId: lastLogId);
            var ipLog = allLogs.LastOrDefault(x => x.Message.Contains("Detected external IP. IP:"));
            var lastLog = allLogs.LastOrDefault();
            if (lastLog is null)
                return JobStateStore.Get<string>(LastExternalIPKey);

            JobStateStore.Set(LastLogIdKey, lastLog.Id);

            if (ipLog is null)
            {
                if (JobStateStore.ContainsKey(LastExternalIPKey))
                    return JobStateStore.Get<string>(LastExternalIPKey);

                logger.LogWarning("No external IP detected in logs");
                return null;
            }
            var ip = ipLog.Message.Split("IP: ")[1].Split(" ")[0].Trim('"');
            logger.LogInformation("Detected external IP: {ip}", ip);
            JobStateStore.Set(LastExternalIPKey, ip);
            return ip;
        }
    }
}
