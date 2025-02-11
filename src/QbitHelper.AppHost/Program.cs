var builder = DistributedApplication.CreateBuilder(args);

var qbittorrent = builder
    .AddContainer("qbittorrent", "ghcr.io/onedr0p/qbittorrent", "5.0.3")
    .WithEnvironment("QBT_WEBUI_PORT", "10095")
    .WithBindMount("config/qBittorrent.conf", "/app/qBittorrent.conf", false)
    .WithHttpEndpoint(10095, 10095, "qbittorrent");

builder
    .AddProject<Projects.QbitHelper_App>("QbitHelperApp")
    .WaitFor(qbittorrent)
    .WithEnvironment($"settings__qbittorrentconfig__host", qbittorrent.GetEndpoint("qbittorrent"));

builder.Build().Run();
