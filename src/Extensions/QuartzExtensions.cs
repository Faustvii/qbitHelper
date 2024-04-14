using Quartz;

namespace QBitHelper.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollectionQuartzConfigurator AddDefaultJob<T>(
            this IServiceCollectionQuartzConfigurator me, JobKey jobKey
        )
            where T : IJob
        {
            return me.AddJob<T>(x =>
                x.WithIdentity(jobKey).StoreDurably().DisallowConcurrentExecution()
            );
        }
    }
}
