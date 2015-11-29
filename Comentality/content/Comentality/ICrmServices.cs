namespace Comentality
{
    using Microsoft.Xrm.Sdk;

    public interface ICrmServices
    {
        ITracingService T { get; set; }
        IOrganizationService Service { get; set; }
        CrmEarlyBound.CrmServiceContext Context { get; set; }
    }
}