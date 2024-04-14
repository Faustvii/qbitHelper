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

var builder = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(x =>
        x.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
    )
    .ConfigureServices(
        (hostContext, services) =>
        {
            services.AddSingleton<QBittorentClientAccessor>();
            services.AddSingleton<PathMappingService>();
            services.Configure<AppConfig>(
                hostContext.Configuration.GetSection(AppConfig.ConfigurationSectionName)
            );
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder
                    .ClearProviders()
                    .AddSerilog(
                        new LoggerConfiguration()
                            .ReadFrom.Configuration(hostContext.Configuration)
                            .CreateLogger()
                    );
            });
            services.AddHttpClient();
            services.AddSingleton<ArrClient>();
            services.AddSingleton(TimeProvider.System);
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.OverWriteExistingData = true;
                options.Scheduling.IgnoreDuplicates = true;
            });
            services.AddQuartz(q =>
                q.AddDefaultJob<MoveOrphanedJob>(MoveOrphanedJob.JobKey)
                    .AddDefaultJob<RemoveOrphanedJob>(RemoveOrphanedJob.JobKey)
                    .AddDefaultJob<InformArrAboutStalledJob>(InformArrAboutStalledJob.JobKey)
                    .AddDefaultJob<TagTorrentPrivacyJob>(TagTorrentPrivacyJob.JobKey)
                    .AddDefaultJob<LimitPublicTorrentSpeedJob>(LimitPublicTorrentSpeedJob.JobKey)
            );
            services.AddQuartzHostedService(x =>
            {
                x.WaitForJobsToComplete = true;
            });
            services.AddHostedService<JobFactoryService>();
        }
    );

var host = builder.Build();
await host.RunAsync();
