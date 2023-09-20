using Quartz;
using Quartz.Impl;

namespace Mensa_Marburg.Scheduler;

public class Scheduler
{
    private ISchedulerFactory schedFact;
    private IScheduler scheduler;
    private IJobDetail job;
    private ITrigger trigger;
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
        
        trigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "myGroup")
            .StartNow()
            .WithCronSchedule("0 0 10 * * ?")
            .Build();
        
        await scheduler.ScheduleJob(job, trigger);
    }
}