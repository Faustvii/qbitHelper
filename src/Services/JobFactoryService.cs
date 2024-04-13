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
        private readonly IOptionsMonitor<SettingsOptions> _optionsAccessor;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<JobFactoryService> _logger;
        private readonly JobChainingJobListener _jobChainingJobListener = new("ChainListener");

        public JobFactoryService(
            IOptionsMonitor<SettingsOptions> optionsAccessor,
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

        public async Task RegisterJobs(SettingsOptions settings)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobConfig = settings.JobConfig;
            if (jobConfig.Orphan.Enabled)
            {
                _logger.LogInformation("Enabling orphaned job");
                var moveTrigger = CreateTrigger(
                    MoveOrphanedJob.JobKey,
                    jobConfig.Orphan.IntervalMinutes
                );
                scheduler.ListenerManager.AddJobListener(
                    _jobChainingJobListener,
                    KeyMatcher<JobKey>.KeyEquals(MoveOrphanedJob.JobKey)
                );
                var jobDetail =
                    await scheduler.GetJobDetail(MoveOrphanedJob.JobKey)
                    ?? JobBuilder
                        .Create<MoveOrphanedJob>()
                        .WithIdentity(MoveOrphanedJob.JobKey)
                        .StoreDurably()
                        .Build();
                await scheduler.ScheduleJob(jobDetail, [moveTrigger], replace: true);
            }
            else
            {
                _logger.LogInformation("Disabling orphaned job");
                var trigger = CreateTrigger(MoveOrphanedJob.JobKey, 0);
                await scheduler.PauseTrigger(trigger.Key);
            }

            if (jobConfig.StalledArr.Enabled)
            {
                _logger.LogInformation("Enabling stalled arr job");
                var stalledArrTrigger = CreateTrigger(
                    InformArrAboutStalledJob.JobKey,
                    jobConfig.StalledArr.IntervalMinutes
                );
                var jobDetail =
                    await scheduler.GetJobDetail(InformArrAboutStalledJob.JobKey)
                    ?? JobBuilder
                        .Create<InformArrAboutStalledJob>()
                        .WithIdentity(InformArrAboutStalledJob.JobKey)
                        .StoreDurably()
                        .Build();
                await scheduler.ScheduleJob(jobDetail, [stalledArrTrigger], replace: true);
            }
            else
            {
                _logger.LogInformation("Disabling stalled arr job");
                var trigger = CreateTrigger(InformArrAboutStalledJob.JobKey, 0);
                await scheduler.PauseTrigger(trigger.Key);
            }

            if (jobConfig.TagTorrentPrivacy.Enabled)
            {
                _logger.LogInformation("Enabling tag torrent privacy job");
                var tagTrigger = CreateTrigger(
                    TagTorrentPrivacyJob.JobKey,
                    jobConfig.TagTorrentPrivacy.IntervalMinutes
                );
                var jobDetail =
                    await scheduler.GetJobDetail(TagTorrentPrivacyJob.JobKey)
                    ?? JobBuilder
                        .Create<TagTorrentPrivacyJob>()
                        .WithIdentity(TagTorrentPrivacyJob.JobKey)
                        .StoreDurably()
                        .Build();
                await scheduler.ScheduleJob(jobDetail, [tagTrigger], replace: true);
            }
            else
            {
                _logger.LogInformation("Disabling tag torrent privacy job");
                var trigger = CreateTrigger(TagTorrentPrivacyJob.JobKey, 0);
                await scheduler.PauseTrigger(trigger.Key);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RegisterJobs(_optionsAccessor.CurrentValue);
        }

        private static ITrigger CreateTrigger(JobKey jobKey, int intervalMinutes)
        {
            return TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(intervalMinutes).RepeatForever())
                .Build();
        }
    }
}
