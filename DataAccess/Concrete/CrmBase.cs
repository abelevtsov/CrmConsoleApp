using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using MoreLinq;
using NLog;

namespace DataAccess.Concrete
{
    public abstract class CrmBase
    {
        protected static readonly Logger Logger = LogManager.GetLogger("RARE");

        private IOrganizationService orgService;

        protected CrmBase(string serviceUriString)
        {
            ServiceUriString = serviceUriString;
        }

        protected IOrganizationService OrgService
        {
            get
            {
                return orgService ?? (orgService = GetOrgService());
            }
        }

        private string ServiceUriString { get; set; }

        protected T ObtainEntityByAttribute<T>(string entityLogicalName, string attributeName, object attributeValue, params string[] columns) where T : Entity
        {
            var query = new QueryExpression(entityLogicalName)
                            {
                                ColumnSet = columns.Any() ? new ColumnSet(columns) : new ColumnSet(false),
                                NoLock = true
                            };
            query.Criteria.AddCondition(attributeName, ConditionOperator.Equal, attributeValue);
            return OrgService.RetrieveMultiple(query).Entities.Cast<T>().FirstOrDefault();
        }

        protected IEnumerable<T> ExecuteMultiple<T>(IEnumerable<T> entities, Func<T, OrganizationRequest> requestBuilder) where T : Entity
        {
            var records = entities as IList<T> ?? entities.ToList();
            var processed = new List<T>();
            foreach (var batch in records.Batch(1000))
            {
                var request =
                    new ExecuteMultipleRequest
                        {
                            Settings =
                                new ExecuteMultipleSettings
                                    {
                                        ContinueOnError = true,
                                        ReturnResponses = true
                                    },
                            Requests = new OrganizationRequestCollection()
                        };
                foreach (var entity in batch)
                {
                    request.Requests.Add(requestBuilder(entity));
                }

                var response = (ExecuteMultipleResponse)OrgService.Execute(request);
                var faultedResponses = response.Responses.Where(responseItem => responseItem.Fault != null).ToList();
                foreach (var faultedResponse in faultedResponses)
                {
                    LogExecuteMultipleFault(faultedResponse.Fault);
                }

                foreach (var responseItem in response.Responses.Where(responseItem => responseItem.Fault == null))
                {
                    var target = GetRequestTarget<T>(request.Requests[responseItem.RequestIndex]);
                    if (responseItem.Response.Results.Any() && responseItem.Response.Results.Contains("id"))
                    {
                        target.Id = (Guid)responseItem.Response.Results["id"];
                    }

                    processed.Add(target);
                }
            }

            return processed;
        }

        protected IEnumerable<T> RetrieveAllEntities<T>(
            string entityLogicalName,
            ColumnSet columnSet = null,
            Action<QueryExpression> addConditionAction = null,
            Action<QueryExpression> addFilterAction = null) where T : Entity
        {
            var query =
                new QueryExpression(entityLogicalName)
                    {
                        ColumnSet = columnSet ?? new ColumnSet(false),
                        NoLock = true,
                        PageInfo =
                            new PagingInfo
                                {
                                    Count = 5000,
                                    PageNumber = 1
                                }
                    };

            if (addConditionAction != null)
            {
                addConditionAction(query);
            }

            if (addFilterAction != null)
            {
                addFilterAction(query);
            }

            var entitiesCollection = OrgService.RetrieveMultiple(query);
            var entities = entitiesCollection.Entities;
            while (entitiesCollection.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = entitiesCollection.PagingCookie;
                entitiesCollection = OrgService.RetrieveMultiple(query);
                entities.AddRange(entitiesCollection.Entities);
            }

            return entities.Cast<T>();
        }

        protected IEnumerable<T> RetrieveAllEntities<T>(QueryExpression query)
        {
            if (query.PageInfo == null)
            {
                query.PageInfo =
                    new PagingInfo
                        {
                            Count = 5000,
                            PageNumber = 1
                        };
            }

            var entitiesCollection = OrgService.RetrieveMultiple(query);
            var entities = entitiesCollection.Entities;
            while (entitiesCollection.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = entitiesCollection.PagingCookie;
                entitiesCollection = OrgService.RetrieveMultiple(query);
                entities.AddRange(entitiesCollection.Entities);
            }

            return entities.Cast<T>();
        }

        private static T GetRequestTarget<T>(OrganizationRequest request) where T : Entity
        {
            var createRequest = request as CreateRequest;
            if (createRequest != null)
            {
                return (T)createRequest.Target;
            }

            var updateRequest = request as UpdateRequest;
            if (updateRequest != null)
            {
                return (T)updateRequest.Target;
            }

            var setStateRequest = request as SetStateRequest;
            if (setStateRequest != null)
            {
                return new Entity(setStateRequest.EntityMoniker.LogicalName)
                           {
                               Id = setStateRequest.EntityMoniker.Id
                           }.ToEntity<T>();
            }

            return default(T);
        }

        private static void LogExecuteMultipleFault(OrganizationServiceFault fault)
        {
            Logger.Error("Произошла ошибка выполнения запроса.\nErrorCode: {0},\nMessage: {1},\nErrorDetails: {2},\nTraceText: {3}",
                         fault.ErrorCode,
                         fault.Message,
                         fault.ErrorDetails,
                         fault.TraceText);
            if (fault.InnerFault != null)
            {
                LogExecuteMultipleFault(fault.InnerFault);
            }
        }

        private IOrganizationService GetOrgService()
        {
            var uri = new Uri(ServiceUriString);
            var clientCredentials = new ClientCredentials();
            clientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
            var serviceProxy =
                    new OrganizationServiceProxy(uri, null, clientCredentials, null)
                        {
                            Timeout = new TimeSpan(0, 5, 0)
                        };
            serviceProxy.Authenticate();
            serviceProxy.EnableProxyTypes();

            return serviceProxy;
        }
    }
}
