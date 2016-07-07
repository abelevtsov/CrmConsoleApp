using System;

using Castle.Core.Internal;
using CrmConsoleApp.Interfaces;
using CrmConsoleApp.Properties;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Windsor;
using Owin;

namespace CrmConsoleApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var ioc = WindsorBootstrapper.BootstrapContainer();

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(
                    Settings.Default.ISVConnectionString,
                    new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(1),
                            JobExpirationCheckInterval = TimeSpan.FromMinutes(1)
                        })
                .UseActivator(new WindsorJobActivator(ioc.Kernel));

            app.UseHangfireDashboard()
               .UseHangfireServer();

            ioc.ResolveAll<IJobRunner>()
               .ForEach(j => j.ProcessRecurringJob());
        }
    }
}
