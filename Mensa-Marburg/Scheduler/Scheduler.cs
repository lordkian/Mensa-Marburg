using Quartz;
using Quartz.Impl;

namespace Mensa_Marburg.Scheduler;

public class Scheduler
{
    private ISchedulerFactory schedFact;
    private IScheduler scheduler;
    private IJobDetail tagJob, nachmittagJob, wocheJob;
    private ITrigger tagTrigger, nachmittagTrigger, wocheTrigger;
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

        tagJob = JobBuilder.Create<CronJob>()
            .WithIdentity("tagJob", "tagGroup")
            .Build();
        nachmittagJob = JobBuilder.Create<CronJob>()
            .WithIdentity("nachmittagJob", "nachmittagGroup")
            .Build();
        wocheJob = JobBuilder.Create<CronJob>()
            .WithIdentity("wocheJob", "wocheGroup")
            .Build();

        tagTrigger = TriggerBuilder.Create()
            .WithIdentity("tagTrigger", "tagGroup")
            .StartNow()
            .WithCronSchedule("0 30 10 ? * MON-FRI") // 10:30 am weekdays
            .Build();

        nachmittagTrigger = TriggerBuilder.Create()
            .WithIdentity("nachmittagTrigger", "nachmittagGroup")
            .StartNow()
            .WithCronSchedule("0 20 14 ? * MON-FRI") // 2:20 pm weekdays
            .Build();

        wocheTrigger = TriggerBuilder.Create()
            .WithIdentity("wocheTrigger", "wocheGroup")
            .StartNow()
            .WithCronSchedule("0 0 10 ? * MON") // 10 am mondays
            .Build();

        await scheduler.ScheduleJob(tagJob,tagTrigger);
        await scheduler.ScheduleJob(nachmittagJob,nachmittagTrigger);
        await scheduler.ScheduleJob(wocheJob,wocheTrigger);
    }
}