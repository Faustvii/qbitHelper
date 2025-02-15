using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QBitHelper;
using QBitHelper.Extensions;
using QBitHelper.Jobs;
using QBitHelper.Services;
using Quartz;
using Serilog;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<QBittorentClientAccessor>();
builder.Services.AddSingleton<PathMappingService>();
builder.Services.Configure<AppConfig>(
    builder.Configuration.GetSection(AppConfig.ConfigurationSectionName)
);
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder
        .ClearProviders()
        .AddSerilog(
            new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger()
        );
});

builder.AddServiceDefaults();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ArrClient>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.OverWriteExistingData = true;
    options.Scheduling.IgnoreDuplicates = true;
});
builder.Services.AddQuartz(q =>
    q.AddDefaultJob<MoveOrphanedJob>(MoveOrphanedJob.JobKey)
        .AddDefaultJob<RemoveOrphanedJob>(RemoveOrphanedJob.JobKey)
        .AddDefaultJob<InformArrAboutStalledJob>(InformArrAboutStalledJob.JobKey)
        .AddDefaultJob<TagTorrentPrivacyJob>(TagTorrentPrivacyJob.JobKey)
        .AddDefaultJob<LimitPublicTorrentSpeedJob>(LimitPublicTorrentSpeedJob.JobKey)
        .AddDefaultJob<ReannounceRacingTorrentJob>(ReannounceRacingTorrentJob.JobKey)
        .AddDefaultJob<TagIssueTorrentsJob>(TagIssueTorrentsJob.JobKey)
        .AddDefaultJob<EnsureQbitPreferencesJob>(EnsureQbitPreferencesJob.JobKey)
        .AddDefaultJob<EnsureQbitConnectableJob>(EnsureQbitConnectableJob.JobKey)
);
builder.Services.AddQuartzHostedService(x =>
{
    x.WaitForJobsToComplete = true;
});

builder.Services.AddHostedService<JobFactoryService>();

builder.Services.AddSingleton<QBittorentClientAccessor>();

var host = builder.Build();
await host.RunAsync();
