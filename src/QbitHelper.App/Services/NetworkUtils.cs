using System.Diagnostics;
using System.Net.Sockets;

namespace QBitHelper.Services;

public static class NetworkUtils
{
    private static readonly ActivitySource ActivitySource = new("QbitHelperApp.NetworkUtils");

    public static async Task<bool> IsConnectable(string address, int port, TimeSpan timeOut)
    {
        using var activity = ActivitySource.StartActivity(
            nameof(IsConnectable),
            ActivityKind.Internal
        );

        if (activity != null)
        {
            activity.SetTag("network.peer.address", address);
            activity.SetTag("network.peer.port", port);
            activity.SetTag("network.timeout_ms", timeOut.TotalMilliseconds);
        }

        try
        {
            using var client = new TcpClient();
            var ct = new CancellationTokenSource(timeOut).Token;
            await client.ConnectAsync(address, port, ct);

            activity?.SetTag("network.connection.success", true);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetTag("network.connection.success", false);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            return false;
        }
    }
}
