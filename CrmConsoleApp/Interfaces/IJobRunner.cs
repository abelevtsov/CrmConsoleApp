namespace CrmConsoleApp.Interfaces
{
    public interface IJobRunner
    {
        string CronExpression { get; }

        void ProcessRecurringJob();
    }
}
