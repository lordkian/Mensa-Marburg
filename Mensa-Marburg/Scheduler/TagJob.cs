using Quartz;

namespace Mensa_Marburg.Scheduler;

public class TagJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.Factory.StartNew(() => { Operator.Instance.Start(); });
    }
}