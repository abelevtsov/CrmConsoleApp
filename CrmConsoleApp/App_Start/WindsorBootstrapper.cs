using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using CrmConsoleApp.Extensions;
using CrmConsoleApp.Interfaces;
using CrmConsoleApp.Properties;

namespace CrmConsoleApp
{
    public static class WindsorBootstrapper
    {
        public static IWindsorContainer BootstrapContainer()
        {
            return
                new WindsorContainer(new XmlInterpreter(new FileResource("castle.config")))
                    .AddFacility<CollectionFacility>()
                    .RegisterRecurringJob<IApplicationsJob>(Settings.Default.AppImportJobCronExpression);
        }
    }
}
