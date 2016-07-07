using System;
using CrmConsoleApp.Interfaces;
using DataAccess.Abstract;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmConsoleApp.Jobs
{
    public class ApplicationsJob : JobBase, IApplicationsJob
    {
        public ApplicationsJob(ICrmRepository crmRepository) : base(crmRepository)
        {
        }

        public override void Run()
        {
            Run(DoImport);
        }

        private void DoImport()
        {
            // CrmRepository.SaveEntities(new[] { new Entity("account") }, e => new UpdateRequest { Target = e });
            Console.WriteLine("do do do");
        }
    }
}
