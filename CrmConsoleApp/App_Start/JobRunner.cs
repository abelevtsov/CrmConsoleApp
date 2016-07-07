using System;

using CrmConsoleApp.Interfaces;
using Hangfire;

namespace CrmConsoleApp
{
    public class JobRunner<T> : IJobRunner where T : IJob
    {
        public JobRunner(string cronExpression)
        {
            CronExpression = cronExpression;
        }

        public JobRunner(T job, string cronExpression)
            : this(cronExpression)
        {
            Job = job;
        }

        public T Job { get; set; }

        public string CronExpression { get; private set; }

        public void ProcessRecurringJob()
        {
            if (Job.Active)
            {
                RecurringJob.AddOrUpdate<T>(Job.JobName, j => j.Run(), CronExpression, TimeZoneInfo.Local);
            }
            else
            {
                RecurringJob.RemoveIfExists(Job.JobName);
            }
        }
    }
}
