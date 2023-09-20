using Quartz;

namespace Mensa_Marburg.Scheduler;

public class CronJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.Factory.StartNew(() =>
        {
             Console.Out.WriteLineAsync("Hello, Quartz.NET!");
        });
    }
}