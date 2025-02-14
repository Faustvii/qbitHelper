using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QBitHelper.Services;

public class PathMappingService
{
    private IOptionsMonitor<AppConfig> _optionsAccessor;
    private readonly ILogger<PathMappingService> _logger;
    private Dictionary<string, string> _localPathMappings;
    private Dictionary<string, string> _remotePathMappings;

    public PathMappingService(
        IOptionsMonitor<AppConfig> optionsAccessor,
        ILogger<PathMappingService> logger
    )
    {
        _optionsAccessor = optionsAccessor;
        _logger = logger;
        _localPathMappings = _optionsAccessor.CurrentValue.PathMappings.ToDictionary(
            x => x.RemotePath,
            x => x.LocalPath
        );
        _remotePathMappings = _optionsAccessor.CurrentValue.PathMappings.ToDictionary(
            x => x.LocalPath,
            x => x.RemotePath
        );
        _optionsAccessor.OnChange(settings =>
        {
            _localPathMappings = settings.PathMappings.ToDictionary(
                x => x.RemotePath,
                x => x.LocalPath
            );
            _remotePathMappings = settings.PathMappings.ToDictionary(
                x => x.LocalPath,
                x => x.RemotePath
            );
        });
    }

    public string MapToLocalPath(string path)
    {
        foreach (var mapping in _localPathMappings)
        {
            if (path.StartsWith(mapping.Key))
            {
                var mappedPath = path.Replace(mapping.Key, mapping.Value);
                _logger.LogTrace("Mapped {path} to {mappedPath}", path, mappedPath);
                return mappedPath;
            }
        }
        return path;
    }

    public string MapToRemotePath(string path)
    {
        foreach (var mapping in _remotePathMappings)
        {
            if (path.StartsWith(mapping.Key))
            {
                var mappedPath = path.Replace(mapping.Key, mapping.Value);
                _logger.LogTrace("Mapped {path} to {mappedPath}", path, mappedPath);
                return mappedPath;
            }
        }
        return path;
    }
}
