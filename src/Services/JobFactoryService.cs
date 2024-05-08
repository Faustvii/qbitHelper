using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QBitHelper.Jobs;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Listener;

namespace QBitHelper.Services
{
    public class JobFactoryService : BackgroundService
    {
        private readonly IOptionsMonitor<AppConfig> _optionsAccessor;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<JobFactoryService> _logger;
        private readonly JobChainingJobListener _jobChainingJobListener = new("ChainListener");

        public JobFactoryService(
            IOptionsMonitor<AppConfig> optionsAccessor,
            ISchedulerFactory schedulerFactory,
            ILogger<JobFactoryService> logger
        )
        {
            _optionsAccessor = optionsAccessor;
            _schedulerFactory = schedulerFactory;
            _logger = logger;
            _optionsAccessor.OnChange(async settings =>
            {
                await RegisterJobs(settings);
            });
            _jobChainingJobListener.AddJobChainLink(
                MoveOrphanedJob.JobKey,
                RemoveOrphanedJob.JobKey
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RegisterJobs(_optionsAccessor.CurrentValue);
        }

        private async Task RegisterJobs(AppConfig settings)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobConfig = settings.JobConfig;
            if (jobConfig.Orphan.Enabled)
            {
                // Add chained job RemoveOrphanedJob to MoveOrphanedJob
                scheduler.ListenerManager.AddJobListener(
                    _jobChainingJobListener,
                    KeyMatcher<JobKey>.KeyEquals(MoveOrphanedJob.JobKey)
                );
            }
            await ManageJob<MoveOrphanedJob>(
                MoveOrphanedJob.JobKey,
                jobConfig.Orphan.Enabled,
                TimeSpan.FromMinutes(jobConfig.Orphan.IntervalMinutes),
                _logger,
                scheduler
            );
            await ManageJob<InformArrAboutStalledJob>(
                InformArrAboutStalledJob.JobKey,
                jobConfig.StalledArr.Enabled,
                TimeSpan.FromMinutes(jobConfig.StalledArr.IntervalMinutes),
                _logger,
                scheduler
            );

            await ManageJob<TagTorrentPrivacyJob>(
                TagTorrentPrivacyJob.JobKey,
                jobConfig.TagTorrentPrivacy.Enabled,
                TimeSpan.FromSeconds(jobConfig.TagTorrentPrivacy.IntervalSeconds),
                _logger,
                scheduler
            );

            await ManageJob<LimitPublicTorrentSpeedJob>(
                LimitPublicTorrentSpeedJob.JobKey,
                jobConfig.LimitPublicTorrentSpeed.Enabled,
                TimeSpan.FromSeconds(jobConfig.LimitPublicTorrentSpeed.IntervalSeconds),
                _logger,
                scheduler
            );

            await ManageJob<ReannounceRacingTorrentJob>(
                ReannounceRacingTorrentJob.JobKey,
                jobConfig.ReannounceRacingTorrent.Enabled,
                TimeSpan.FromSeconds(jobConfig.ReannounceRacingTorrent.IntervalSeconds),
                _logger,
                scheduler
            );

            await ManageJob<TagIssueTorrentsJob>(
                TagIssueTorrentsJob.JobKey,
                jobConfig.TagIssueTorrent.Enabled,
                TimeSpan.FromSeconds(jobConfig.TagIssueTorrent.IntervalSeconds),
                _logger,
                scheduler
            );

            await ManageJob<EnsureQbitPreferencesJob>(
                EnsureQbitPreferencesJob.JobKey,
                jobConfig.EnsureQbitPreferences.Enabled,
                TimeSpan.FromSeconds(jobConfig.EnsureQbitPreferences.IntervalSeconds),
                _logger,
                scheduler
            );
        }

        public static async Task ManageJob<T>(
            JobKey jobKey,
            bool enabled,
            TimeSpan interval,
            ILogger logger,
            IScheduler scheduler
        )
            where T : IJob
        {
            if (enabled)
            {
                logger.LogInformation("Enabling {jobKey} job", jobKey.Name);
                var trigger = CreateTrigger(jobKey, interval);
                var jobDetail =
                    await scheduler.GetJobDetail(jobKey)
                    ?? JobBuilder.Create<T>().WithIdentity(jobKey).StoreDurably().Build();
                await scheduler.ScheduleJob(jobDetail, [trigger], replace: true);
            }
            else
            {
                logger.LogInformation("Disabling {jobKey} job", jobKey.Name);
                var trigger = CreateTrigger(jobKey, TimeSpan.MaxValue);
                await scheduler.PauseTrigger(trigger.Key);
            }
        }

        private static ITrigger CreateTrigger(JobKey jobKey, TimeSpan interval)
        {
            return TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();
        }
    }
}
