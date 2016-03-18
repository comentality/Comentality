using CrmEarlyBound;

namespace Comentality
{
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CommonRepository : ICommonRepository
    {
        public CommonRepository(ICrmServices services)
        {
            this.services = services;
        }

        public void SetState(EntityReference id, OptionSetValue statusCode, OptionSetValue stateCode)
        {
            var setStateRR = new SetStateRequest
            {
                State = stateCode,
                Status = statusCode,
                EntityMoniker = id
            };

            this.services.Service.Execute(setStateRR);
        }

        [Obsolete("Use `GetFullById<T>(EntityReference id)` if you want to get full entity.")]
        public T RetrieveById<T>(EntityReference id) where T : Entity
        {
            return GetFullById<T>(id);
        }

        /// <summary>
        /// Get full entity by Id.
        /// </summary>
        /// <typeparam name="T">Type of entity to retrieve</typeparam>
        /// <param name="id">Entity Id.</param>
        /// <exception cref="InvalidPluginExecutionException">Exception if entity is not found.</exception>
        /// <returns>Full entity value.</returns>
        public T GetFullById<T>(EntityReference id) where T : Entity
        {
            var entity =
                this.services.Context
                .CreateQuery<T>()
                .SingleOrDefault(x =>
                    x.Id == id.Id);

            if (entity == null)
            {
                throw new InvalidPluginExecutionException($"EntityNotFoundException: No {typeof(T)} with id {id} ");
            }

            return entity;
        }

        /// <summary>
        /// Constructs query to get this type of Entity by Id. You need to specify select statement and enumerate.
        /// </summary>
        /// <typeparam name="T">Enity Type.</typeparam>
        /// <param name="id">Entity Id.</param>
        /// <returns>Returns query to get this type of Entity by Id.</returns>
        public IQueryable<T> GetById<T>(EntityReference id) where T : Entity
        {
            var entity =
                this.services.Context
                .CreateQuery<T>()
                .Where(x =>
                    x.Id == id.Id);

            return entity;
        }

        public List<TransactionCurrency> RetrieveTransactionCurrencies()
        {
            return this.services.Context.TransactionCurrencySet.ToList();
        }

        public Guid RetrieveBaseCurrency()
        {
            var org = this.services.Context.OrganizationSet.Single();

            return org.BaseCurrencyId.Id;
        }

        public EntityReference RetrievePriceListOrNull(string priceListName)
        {
            Throwers.IfNullOrEmptyArgument(priceListName, "priceListName");

            var priceListGuid =
                this.services.Context
                .CreateQuery<PriceLevel>()
                .Where(x => x.Name == priceListName)
                .Select(x => x.Id)
                .FirstOrDefault();

            if (priceListGuid == Guid.Empty)
            {
                return null;
            }

            return new EntityReference(PriceLevel.EntityLogicalName, priceListGuid);
        }

        public EntityReference CreatePriceList(string priceListName, EntityReference currencyId)
        {
            this.services.T.Trace("CreatePriceList...");

            Throwers.IfNullOrEmptyArgument(priceListName, "priceListName");
            Throwers.IfNullArgument(currencyId, "currencyId");
            Throwers.IfReferenceTypeIsWrong(currencyId, TransactionCurrency.EntityLogicalName);

            var newPriceLevel =
                new PriceLevel
                {
                    Name = priceListName,
                    TransactionCurrencyId = currencyId
                };

            var id = this.services.Service.Create(newPriceLevel);

            this.services.T.Trace("CreatePriceList!");

            return new EntityReference(PriceLevel.EntityLogicalName, id);
        }

        public EntityReference Create(Entity entity)
        {
            Throwers.IfNullArgument(entity, "entity");

            var id = this.services.Service.Create(entity);

            return new EntityReference(entity.LogicalName, id);
        }

        public void Update(Entity entity)
        {
            Throwers.IfNullArgument(entity, "entity");

            this.services.Service.Update(entity);
        }

        public static List<ActivityParty> SpawnActivityParty(EntityReference managerId)
        {
            return new List<ActivityParty>
            {
                new ActivityParty
                {
                    PartyId = managerId
                }
            };
        }

        public void Trace(string format, params object[] args)
        {
            this.services.T?.Trace(format, args);
        }

#region Email
        public void Email(string subject, string body, EntityReference regarding, EntityReference to, EntityReference from, Dictionary<string, byte[]> attachments = null)
        {
            Email(subject, body, regarding, new List<EntityReference> { to }, from, attachments);
        }

        public void Email(string subject, string body, EntityReference regarding, EntityReference to, EntityReference from, Dictionary<string, string> attachments = null)
        {
            Email(subject, body, regarding, new List<EntityReference> { to }, from, attachments);
        }

        public void Email(string subject, string body, EntityReference regarding, IEnumerable<EntityReference> to, EntityReference from, Dictionary<string, byte[]> attachments = null)
        {
            var atts = new Dictionary<string, string>();

            if (attachments != null)
            {
                atts =
                    attachments
                    .ToDictionary(
                        x => x.Key,
                        x => Convert.ToBase64String(x.Value));
            }

            Email(subject, body, regarding, to , from, atts);
        }

        public void Email(string subject, string body, EntityReference regarding, IEnumerable<EntityReference> to, EntityReference from, Dictionary<string, string> attachments = null )
        {
            var email = new Email
            {
                Subject = subject,
                Description = body,
                From = new List<ActivityParty> {
                    new ActivityParty {
                        PartyId = from
                    }
                },
                To = to.Select(x => new ActivityParty
                {
                    PartyId = x
                }),
                RegardingObjectId = regarding
            };

           var newId = this.services.Service.Create(email);

            if (attachments != null)
            {
                var att =
                    attachments.Select(x =>
                    new ActivityMimeAttachment
                    {
                        ObjectId = new EntityReference(CrmEarlyBound.Email.EntityLogicalName, newId),
                        ObjectTypeCode = CrmEarlyBound.Email.EntityLogicalName,
                        Subject = x.Key,
                        Body = x.Value,
                        FileName = x.Key
                    });

                foreach (var attachment in att)
                {
                    this.services.Service.Create(attachment);
                }
            }

            var sndEmail = new SendEmailRequest
            {
                EmailId = newId,
                IssueSend = true,
                TrackingToken = ""
            };

            this.services.Service.Execute(sndEmail);
        }
#endregion

        #region private

        protected ICrmServices services;

        #endregion private
    }
}
