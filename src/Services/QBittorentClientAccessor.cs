using Microsoft.Extensions.Options;
using QBittorrent.Client;

namespace QBitHelper.Services;

public class QBittorentClientAccessor
{
    private readonly IOptionsMonitor<SettingsOptions> _optionsAccessor;
    private QBittorrentClient _client;

    public QBittorentClientAccessor(IOptionsMonitor<SettingsOptions> optionsAccessor)
    {
        _optionsAccessor = optionsAccessor;
        _client = new QBittorrentClient(
            new Uri(_optionsAccessor.CurrentValue.QbittorrentConfig.Host),
            ApiLevel.V2
        );
        _optionsAccessor.OnChange(settings =>
            _client = new QBittorrentClient(new Uri(settings.QbittorrentConfig.Host), ApiLevel.Auto)
        );
    }

    public async Task<QBittorrentClient> GetClient(CancellationToken cancellationToken = default)
    {
        if (
            _optionsAccessor.CurrentValue.QbittorrentConfig.Username != null
            && _optionsAccessor.CurrentValue.QbittorrentConfig.Password != null
        )
        {
            await _client.LoginAsync(
                _optionsAccessor.CurrentValue.QbittorrentConfig.Username,
                _optionsAccessor.CurrentValue.QbittorrentConfig.Password,
                cancellationToken
            );
        }

        return _client;
    }
}
