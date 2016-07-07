using System;

using CrmConsoleApp.Interfaces;
using DataAccess.Abstract;
using NLog;
using Polly;

namespace CrmConsoleApp.Jobs
{
    public abstract class JobBase : IJob
    {
        protected static readonly Logger Logger = LogManager.GetLogger("RARE");

        protected JobBase(ICrmRepository crmRepository)
        {
            CrmRepository = crmRepository;
        }

        public string JobName { get; set; }

        public bool Active { get; set; }

        protected ICrmRepository CrmRepository { get; private set; }

        public abstract void Run();

        protected static void Run(params Action[] actions)
        {
            const int retryCount = 3;
            foreach (var action in actions)
            {
                var localAction = action;
                var actionName = string.Format("{0}.{1}", localAction.Method.DeclaringType.FullName, localAction.Method.Name);
                try
                {
                    Policy.Handle<Exception>()
                        .WaitAndRetry(
                            retryCount,
                            i => TimeSpan.FromSeconds(i),
                            (ex, timeSpan, context) =>
                            {
                                Logger.Error(ex, actionName);
                            })
                        .Execute(localAction);
                }
                // ReSharper disable once CatchAllClause
                catch (Exception ex)
                {
                    Logger.Fatal(ex, actionName);
                    // ReSharper disable once ExceptionNotDocumented
                    // ReSharper disable once ThrowingSystemException
                    throw;
                }
            }
        }
    }
}
