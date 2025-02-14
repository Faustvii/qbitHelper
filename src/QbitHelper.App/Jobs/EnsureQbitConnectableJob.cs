using System.Net.Sockets;
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

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = optionsAccessor.CurrentValue.QbittorrentConfig.Preferences;
            var client = await qBittorentClientAccessor.GetClient();
            var preferences = await client.GetPreferencesAsync();
            var dryRun = optionsAccessor.CurrentValue.DryRun;

            var nicPreference = preferences.CurrentNetworkInterface;
            if (string.IsNullOrWhiteSpace(nicPreference))
            {
                logger.LogInformation("No specific network interface set in preferences");
                return;
            }

            // var externalIp = await ExtractExternalIP(client);

            logger.LogInformation(
                "Current network interface is {currentNic} - {address}",
                nicPreference,
                preferences.CurrentInterfaceAddress
            );
            var nicAddresses = await client.GetNetworkInterfaceAddressesAsync(nicPreference);
            logger.LogInformation(
                "Network interface {nicName} has {nicAddresses} addresses",
                nicPreference,
                nicAddresses.Count
            );

            var nicAddress = nicAddresses[0];
            logger.LogInformation(
                "Using network interface {nicName} with address {nicAddress}",
                nicPreference,
                nicAddress
            );

            logger.LogInformation(
                "Testing Qbittorrent connection on {ip}:{port}",
                nicAddress,
                settings.ListenPort
            );
            var isConnectable = await IsIPAndPortOpen(
                nicAddress,
                settings.ListenPort,
                TimeSpan.FromSeconds(5)
            );
            if (isConnectable)
            {
                logger.LogInformation("Qbittorrent is connectable: {isConnectable}", isConnectable);
                return;
            }
            logger.LogWarning(
                "Qbittorrent is not connectable - triggering network interface update in an attempt to fix"
            );
            // Trigger network interface update in Qbit so it binds again
            if (dryRun)
                return;

            await client.SetPreferencesAsync(
                new Preferences
                {
                    CurrentNetworkInterface = nicPreference,
                    CurrentInterfaceAddress =
                        preferences.CurrentInterfaceAddress == string.Empty
                            ? "0.0.0.0"
                            : string.Empty,
                }
            );
        }

        private async Task<string?> ExtractExternalIP(QBittorrentClient client)
        {
            var allLogs = await client.GetLogAsync(TorrentLogSeverity.Info);
            var ipLog = allLogs.LastOrDefault(x => x.Message.Contains("Detected external IP. IP:"));
            if (ipLog is null)
            {
                logger.LogWarning("No external IP detected in logs");
                return null;
            }
            var ip = ipLog.Message.Split("IP: ")[1].Split(" ")[0].Trim('"');
            logger.LogInformation("Detected external IP: {ip}", ip);
            return ip;
        }

        public static async Task<bool> IsIPAndPortOpen(
            string hostOrIPAddress,
            int port,
            TimeSpan timeOut
        )
        {
            try
            {
                using var client = new TcpClient();
                var ct = new CancellationTokenSource(timeOut).Token;
                await client.ConnectAsync(hostOrIPAddress, port, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
