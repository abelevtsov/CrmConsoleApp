﻿using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;

namespace CrmConsoleApp
{
    public class CollectionFacility : IFacility
    {
        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            kernel.Resolver.AddSubResolver(new CollectionResolver(kernel));
        }

        public void Terminate()
        {
        }
    }
}
