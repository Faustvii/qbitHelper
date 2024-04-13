using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QBitHelper;
using QBitHelper.Jobs;
using QBitHelper.Services;
using Quartz;
using Serilog;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(
        (hostContext, services) =>
        {
            services.AddSingleton<QBittorentClientAccessor>();
            services.AddSingleton<PathMappingService>();
            services.Configure<SettingsOptions>(
                hostContext.Configuration.GetSection(SettingsOptions.ConfigurationSectionName)
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
                q.AddJob<MoveOrphanedJob>(j =>
                        j.WithIdentity(MoveOrphanedJob.JobKey)
                            .StoreDurably()
                            .DisallowConcurrentExecution()
                    )
                    .AddJob<RemoveOrphanedJob>(j =>
                        j.WithIdentity(RemoveOrphanedJob.JobKey)
                            .StoreDurably()
                            .DisallowConcurrentExecution()
                    )
                    .AddJob<InformArrAboutStalledJob>(j =>
                        j.WithIdentity(InformArrAboutStalledJob.JobKey)
                            .StoreDurably()
                            .DisallowConcurrentExecution()
                    )
                    .AddJob<TagTorrentPrivacyJob>(j =>
                        j.WithIdentity(TagTorrentPrivacyJob.JobKey)
                            .StoreDurably()
                            .DisallowConcurrentExecution()
                    )
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
