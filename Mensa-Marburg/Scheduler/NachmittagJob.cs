using Quartz;

namespace Mensa_Marburg.Scheduler;

public class NachmittagJob: IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.Factory.StartNew(() => { });
    }
}