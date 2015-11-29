using System;
using CrmEarlyBound;
using Microsoft.Xrm.Sdk;

namespace Comentality
{
    using Exceptions;

    public class PluginHelper
    {
        private IServiceProvider serviceProvider;
        private IPluginExecutionContext context;
        private Entity target;
        private CrmServices services;
        private readonly string defaultImageName = "Image";
        private EntityReference id;

        public PluginHelper(IServiceProvider serviceProvider, string defaultImageName = "Image")
        {
            this.defaultImageName = defaultImageName;


            this.serviceProvider = serviceProvider;

            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //((IProxyTypesAssemblyProvider)this.context).ProxyTypesAssembly = typeof(Contact).Assembly;
        }

        public CrmServices Services
        {
            get
            {
                if (this.services == null)
                {
                    this.services = GetServices();
                }

                return this.services;
            }
        }

        public EntityReference Id => id ?? (id = new EntityReference(this.context.PrimaryEntityName, this.context.PrimaryEntityId));

        /// <summary>
        /// Gets the name of the Plugin Message that is being processed by the event execution pipe.
        /// </summary>
        public string MessageName
        {
            get { return this.context.MessageName; }
        }

        public void CheckRegistration(object registration)
        {
            // TODO(commentality): check registration!!!
            // throw new NotImplementedException();
        }

        public Entity GetTarget()
        {
            this.target = (Entity)this.context.InputParameters["Target"];

            return this.target;
        }

        public T GetTarget<T>() where T : Entity
        {
            if (!this.context.InputParameters.ContainsKey("Target"))
            {
                throw new NoTargetException("Plugin has no target. Processed message might has no Target (ex. Delete).");
            }

            this.target = this.GetTarget();

            return this.target.ToEntity<T>();
        }

        public T GetPreImage<T>(string imageName = "Image") where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (this.context.PreEntityImages.Contains(imageName))
            {
                return this.context.PreEntityImages[imageName].ToEntity<T>();
            }

            throw new NoImageException(NoImageException.BuildMessage(imageName, 1));
        }

        public T GetPostImage<T>(string imageName = "Image") where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (this.context.PostEntityImages.Contains(imageName))
            {
                return this.context.PostEntityImages[imageName].ToEntity<T>();
            }

            throw new NoImageException(NoImageException.BuildMessage(imageName, 0));
        }

        /// <summary>
        /// Returns types Pre Image or Target if this Pre Image is not available.
        /// </summary>
        /// <typeparam name="T">Type of Image.</typeparam>
        /// <returns>Returns Pre Image or Target.</returns>
        public T GetPreImageOrTarget<T>(string imageName = "Image") where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (context.PreEntityImages.ContainsKey(imageName))
            {
                return this.GetPreImage<T>(imageName);
            }

            return this.GetTarget<T>();
        }

        private CrmServices GetServices()
        {
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service = serviceFactory.CreateOrganizationService(context.UserId);

            var t = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            return GetServices(service, t);
        }

        public static CrmServices GetServices(IOrganizationService service, ITracingService t)
        {
            var xrmContext = new CrmServiceContext(service);


            return new CrmServices(service, xrmContext, t);
        }

        private string FallbackToDefaultName(string imageName)
        {
            return
                !string.IsNullOrEmpty(imageName)
                ? imageName
                : this.defaultImageName;
        }
    }
}
