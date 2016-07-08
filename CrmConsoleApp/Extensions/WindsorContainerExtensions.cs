using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CrmConsoleApp.Interfaces;

namespace CrmConsoleApp.Extensions
{
    public static class WindsorContainerExtensions
    {
        public static IWindsorContainer RegisterRecurringJob<T>(this IWindsorContainer container, string cronExpression) where T : IJob
        {
            container.Register(Component.For<IJobRunner>()
                     .ImplementedBy<JobRunner<T>>()
                     .LifeStyle.Transient
                     .DependsOn(Dependency.OnValue("cronExpression", cronExpression)));

            return container;
        }
    }
}
