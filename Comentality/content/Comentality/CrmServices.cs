namespace Comentality
{
    using Microsoft.Xrm.Sdk;
    using CrmEarlyBound;

    public sealed class CrmServices : ICrmServices
    {
        public ITracingService T { get; set; }

        public IOrganizationService Service { get; set; }

        public CrmServiceContext Context { get; set; }

        public CrmServices(IOrganizationService service, CrmServiceContext xrmContext, ITracingService t)
        {
            this.Service = service;
            this.T = t;
            this.Context = xrmContext;
        }
    }
}
