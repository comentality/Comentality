using System;
using Comentality.content.Comentality;
using CrmEarlyBound;
using Microsoft.Xrm.Sdk;

namespace Comentality
{
    public class PluginHelper
    {
        private readonly IServiceProvider serviceProvider;
        public readonly IPluginExecutionContext PluginExecutionContext;
        private Entity target;
        private CrmServices services;
        private static string DefaultImageName = "Image";
        private static string ImageName = "Image";
        private EntityReference id;
        private const string TARGET = "Target";

        public PluginHelper(IServiceProvider serviceProvider, string imageName = null)
        {
            ImageName = this.FallbackToDefaultName(imageName);

            this.serviceProvider = serviceProvider;

            this.PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

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

        public EntityReference Id => id ?? (id = new EntityReference(this.PluginExecutionContext.PrimaryEntityName, this.PluginExecutionContext.PrimaryEntityId));

        /// <summary>
        /// Gets the name of the Plugin Message that is being processed by the event execution pipe.
        /// </summary>
        public string MessageName => this.PluginExecutionContext.MessageName;


        public void CheckRegistration(object registration)
        {
            // TODO(commentality): check registration!!!
            // throw new NotImplementedException();
        }


        public Entity GetTarget()
        {
            this.target = (Entity)this.PluginExecutionContext.InputParameters[TARGET];

            return this.target;
        }


        public T GetTarget<T>() where T : Entity
        {
            if (!this.PluginExecutionContext.InputParameters.ContainsKey(TARGET))
            {
                throw new InvalidPluginExecutionException("Plugin has no target. Processed message might has no Target (ex. Delete).");
            }

            this.target = this.GetTarget();

            return this.target.ToEntity<T>();
        }

        public T GetTargetOrNull<T>() where T : Entity
        {
            if (!this.PluginExecutionContext.InputParameters.ContainsKey(TARGET) ||
                this.PluginExecutionContext.InputParameters[TARGET] is EntityReference)
            {
                return null;
            }

            this.target = this.GetTarget();

            return this.target.ToEntity<T>();
        }


        public T GetPreImage<T>(string imageName = null) where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (this.PluginExecutionContext.PreEntityImages.Contains(imageName))
            {
                return this.PluginExecutionContext.PreEntityImages[imageName].ToEntity<T>();
            }

            throw new InvalidPluginExecutionException(BuildNoImageMessage(imageName, ImageType.PreImage));
        }


        public T GetPostImage<T>(string imageName = null) where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (this.PluginExecutionContext.PostEntityImages.Contains(imageName))
            {
                return this.PluginExecutionContext.PostEntityImages[imageName].ToEntity<T>();
            }

            throw new InvalidPluginExecutionException(BuildNoImageMessage(imageName, ImageType.PostImage));
        }


        /// <summary>
        /// Returns types Pre Image or Target if this Pre Image is not available.
        /// </summary>
        /// <typeparam name="T">Type of Image.</typeparam>
        /// <returns>Returns Pre Image or Target.</returns>
        public T GetPreImageOrTarget<T>(string imageName = null) where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);

            if (PluginExecutionContext.PreEntityImages.ContainsKey(imageName))
            {
                return this.GetPreImage<T>(imageName);
            }

            return this.GetTarget<T>();
        }


        public static CrmServices GetServices(IOrganizationService service, ITracingService t)
        {
            var xrmContext = new CrmServiceContext(service);


            return new CrmServices(service, xrmContext, t);
        }


        public T GetPostOrPreImage<T>(string imageName = null) where T : Entity
        {
            imageName = this.FallbackToDefaultName(imageName);


            if (PluginExecutionContext.PostEntityImages.ContainsKey(imageName))
            {
                return this.GetPostImage<T>(imageName);
            }

            if (PluginExecutionContext.PreEntityImages.ContainsKey(imageName))
            {
                return this.GetPreImage<T>(imageName);
            }

            return null;
        }







        private CrmServices GetServices()
        {
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service = serviceFactory.CreateOrganizationService(PluginExecutionContext.UserId);

            var t = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            return GetServices(service, t);
        }

        private string FallbackToDefaultName(string imageName)
        {
            return
                !string.IsNullOrEmpty(imageName)
                ? imageName
                : PluginHelper.ImageName;
        }

        private static string BuildNoImageMessage(string imageName, ImageType imageType)
        {
            string message;

            if (imageName == PluginHelper.DefaultImageName)
            {
                message = string.Format(
                    $"Failed to read {imageType.Readable()} '{imageName}'. " +
                    $"'{imageName}' is default name for an image. " +
                    "If you want to use other name -- you should pass it as argument `PluginHelper.GetPreImage<T>(\"image_name\")` @ +" +
                    "or as a second argument of `PluginHelper` constructor. " +
                    $"Doublecheck that '{PluginHelper.ImageName}' image is registered, is {imageType.Readable()}, has this exact name.");
            }
            else
            {
                message = string.Format(
                    $"Failed to read {imageType.Readable()} '{imageName}'. " +
                    $"Doublecheck that '{imageName}' image is registered, is {imageType.Readable()}, has this exact name.");
            }

            return message;
        }
    }
}
