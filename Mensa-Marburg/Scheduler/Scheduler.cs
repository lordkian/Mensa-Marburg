using Quartz;
using Quartz.Impl;

namespace Mensa_Marburg.Scheduler;

public class Scheduler
{
    private ISchedulerFactory schedFact;
    private IScheduler scheduler;
    private IJobDetail job;
    private ITrigger trigger1 , trigger2;
    public static Scheduler Instance;

    static Scheduler()
    {
        Instance = new Scheduler();
    }

    public async void start()
    {
        // Initialize scheduler
        schedFact = new StdSchedulerFactory();
        scheduler = await schedFact.GetScheduler();
        await scheduler.Start();

        job = JobBuilder.Create<CronJob>()
            .WithIdentity("myJob", "myGroup")
            .Build();

        trigger1 = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "myGroup")
            .StartNow()
            .WithCronSchedule("0 0 10 * * ?")// 10 am
            .Build();
        
        trigger1 = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "myGroup")
            .StartNow()
            .WithCronSchedule("0 20 14 * * ?")// 2:20 pm
            .Build();

        await scheduler.ScheduleJob(job,
            new HashSet<ITrigger>() { trigger1, trigger2 }, replace: true);
    }
}