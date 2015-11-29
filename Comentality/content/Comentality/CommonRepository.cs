using CrmEarlyBound;

namespace Comentality
{
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exceptions;

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

        public T RetrieveById<T>(EntityReference id) where T : Entity
        {
            var entity =
                this.services.Context
                .CreateQuery<T>()
                .SingleOrDefault(x =>
                    x.Id == id.Id);

            if (entity == null)
            {
                throw new EntityNotFoundException();
            }

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
            Throwers.ThrowOnNullOrEmptyArgument(priceListName, "priceListName");

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

            Throwers.ThrowOnNullOrEmptyArgument(priceListName, "priceListName");
            Throwers.ThrowOnNullArgument(currencyId, "currencyId");
            Throwers.ThrowOnWrongReferenceType(currencyId, TransactionCurrency.EntityLogicalName);

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
            Throwers.ThrowOnNullArgument(entity, "entity");

            var id = this.services.Service.Create(entity);

            return new EntityReference(entity.LogicalName, id);
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

        #region private

        protected ICrmServices services;

        #endregion private
    }
}
