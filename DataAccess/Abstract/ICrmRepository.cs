using System;
using System.Collections.Generic;

using Microsoft.Xrm.Sdk;

namespace DataAccess.Abstract
{
    public interface ICrmRepository
    {
        IEnumerable<Guid> SaveEntities<T>(IEnumerable<T> entities, Func<T, OrganizationRequest> requestBuilder) where T : Entity;
    }
}
