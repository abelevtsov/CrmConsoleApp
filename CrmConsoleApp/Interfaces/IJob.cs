namespace CrmConsoleApp.Interfaces
{
    public interface IJob
    {
        string JobName { get; }

        bool Active { get; }

        void Run();
    }
}
