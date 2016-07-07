using System;
using System.Collections.Generic;
using System.Linq;

using DataAccess.Abstract;
using Microsoft.Xrm.Sdk;

namespace DataAccess.Concrete
{
    public class CrmRepository : CrmBase, ICrmRepository
    {
        public CrmRepository(string serviceUriString) : base(serviceUriString)
        {
        }

        public IEnumerable<Guid> SaveEntities<T>(IEnumerable<T> entities, Func<T, OrganizationRequest> requestBuilder) where T : Entity
        {
            return ExecuteMultiple(entities, requestBuilder).Select(e => e.Id);
        }
    }
}
