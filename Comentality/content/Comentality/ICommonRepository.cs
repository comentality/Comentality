using CrmEarlyBound;

namespace Comentality
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk;
    using System;

    public interface ICommonRepository
    {
        T RetrieveById<T>(EntityReference id) where T : Entity;

        void SetState(EntityReference id, OptionSetValue statusCode, OptionSetValue stateCode);
        
        List<TransactionCurrency> RetrieveTransactionCurrencies();
        
        Guid RetrieveBaseCurrency();

        EntityReference RetrievePriceListOrNull(string priceListName);

        EntityReference CreatePriceList(string priceListName, EntityReference currencyId);
    }
}
