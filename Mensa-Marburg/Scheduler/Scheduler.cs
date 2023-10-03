using System.Collections.ObjectModel;
using Quartz;
using Quartz.Impl;

namespace Mensa_Marburg.Scheduler;

public class Scheduler
{
    private ISchedulerFactory schedFact;
    private IScheduler scheduler;
    private IJobDetail tagJob;
    private ITrigger tagTrigger, nachmittagTrigger;
    public static readonly Scheduler Instance;

    static Scheduler()
    {
        Instance = new Scheduler();
    }

    private Scheduler()
    {
    }

    public async void Init()
    {
        // Initialize scheduler
        schedFact = new StdSchedulerFactory();
        scheduler = await schedFact.GetScheduler();
        await scheduler.Start();

        InitJobsAndTrigers();

        StartSchedule();
    }

    public async void StartSchedule()
    {
        var list = new List<ITrigger>() { tagTrigger, nachmittagTrigger };
        await scheduler.ScheduleJob(tagJob, new ReadOnlyCollection<ITrigger>(list), true);
    }

    public async void StopSchedule()
    {
        await scheduler.Shutdown();
    }

    public async void PauseSchedule()
    {
        await scheduler.PauseAll();
    }

    public async void ResumeSchedule()
    {
        await scheduler.ResumeAll();
    }

    private void InitJobsAndTrigers()
    {
        tagJob = JobBuilder.Create<TagJob>()
            .WithIdentity("tagJob", "tagGroup")
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
    }
}