using Quartz;

namespace Mensa_Marburg.Scheduler;

public class WocheJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.Factory.StartNew(() => { Operator.Instance.PostToWochePlanChannel(); });
    }
}