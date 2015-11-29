using System.Collections.ObjectModel;
using Comentality.Exceptions;
using CrmEarlyBound;

namespace Comentality.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Transaction Currencies API.
    /// </summary>
    public class Currencies
    {
        #region ctors

        /// <summary>
        /// Transaction Currencies API.
        /// </summary>
        /// <param name="repo">ICommonRepository.</param>
        public Currencies(ICommonRepository repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// All transaction Currencies.
        /// </summary>
        public ReadOnlyCollection<TransactionCurrency> TransactionCurrencies
        {
            get
            {
                var ret = transactionCurrencies ??
                    (transactionCurrencies = this.repo.RetrieveTransactionCurrencies());

                return ret.AsReadOnly();
            }
        }
        #endregion ctors
        

        protected readonly ICommonRepository repo;
        protected Guid? baseCurrencyId;
        protected List<TransactionCurrency> transactionCurrencies;


        /// <summary>
        /// USD Transaction currency.
        /// </summary>
        public TransactionCurrency USD
        {
            get
            {
                return this.GetCurrency(CurrencyIsoCode.USD);
            }
        }

        /// <summary>
        /// Base Currency of CRM.
        /// </summary>
        public TransactionCurrency BaseCurrency
        {
            get
            {
                if (baseCurrencyId == null)
                {
                    baseCurrencyId = this.repo.RetrieveBaseCurrency();
                }

                return this.GetCurrency(baseCurrencyId.Value);
            }
        }

        /// <summary>
        /// Check if currency exists in CRM.
        /// </summary>
        /// <param name="isoCode">Currency ISO code.</param>
        /// <returns>Returns true if currencies exist in CRM.</returns>
        public bool CurrencyExists(string isoCode)
        {
            return this.TransactionCurrencies.Any(x => x.ISOCurrencyCode == isoCode);
        }

        /// <summary>
        /// Get Currency by ISO code.
        /// </summary>
        /// <param name="isoCode">Currency ISO code.</param>
        /// <returns>Returns full Transaction Currency record.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <param name="isoCode">isoCode</param> is empty.
        /// </exception>
        /// <exception cref="EntityNotFoundException">
        /// Thrown when there is no such Currency.</exception>
        public TransactionCurrency GetCurrency(string isoCode)
        {
            Throwers.ThrowOnNullArgument(isoCode, "isoCode");

            var cur = this.TransactionCurrencies.SingleOrDefault(x => x.ISOCurrencyCode == isoCode);

            if (cur == null)
            {
                throw new EntityNotFoundException("Currency with ISO code " + " does not exist in CRM.");
            }

            return cur;
        }

        /// <summary>
        /// Get Transaction Currency by Id.
        /// </summary>
        /// <param name="currencyId">Currency Id.</param>
        /// <returns>Returns Transaction Currency.</returns>
        public TransactionCurrency GetCurrency(Guid currencyId)
        {
            var cur = this.TransactionCurrencies.SingleOrDefault(x => x.TransactionCurrencyId == currencyId);

            if (cur == null)
            {
                throw new EntityNotFoundException("Currency with ISO code " + " does not exist in CRM.");
            }

            return cur;
        }

        #region private functions

        private decimal GetExhangeRate(string isoCode)
        {
            var currency = this.GetCurrency(isoCode);

            if (currency == null)
            {
                throw new InvalidPluginExecutionException("No " + isoCode + " found in organization!");
            }

            if (currency.ExchangeRate == null)
            {
                throw new InvalidPluginExecutionException("No exchange rate set for " + isoCode + "!");
            }

            var exchangeRate = currency.ExchangeRate.Value;

            return exchangeRate;
        }

        private decimal GetExhangeRate(Guid currencyId)
        {
            var currency = this.GetCurrency(currencyId);


            if (currency.ExchangeRate == null)
            {
                throw new UnexpectedNullValueException("No exchange rate set for " + currency.ISOCurrencyCode + "!");
            }

            return currency.ExchangeRate.Value;
        }

        /// <summary>
        /// Formula for conversion: base to target currency.
        /// </summary>
        /// <param name="amountInBaseCurrency">Amount in base currency.</param>
        /// <param name="targetCurrencyExchangeRate">Target currency exchange rate.</param>
        /// <returns>Returns amount converted from base currency with exchange rate.</returns>
        protected decimal FromBase(decimal amountInBaseCurrency, decimal targetCurrencyExchangeRate)
        {
            return
                amountInBaseCurrency * targetCurrencyExchangeRate;
        }

        /// <summary>
        /// Formula for conversion: source to base currency.
        /// </summary>
        /// <param name="amountInSourceCurrency">Amount in source currency.</param>
        /// <param name="sourceCurrencyExchangeRate">Source currency exchange rate.</param>
        /// <returns>Returns amount converted to base currency with exchange rate.</returns>
        protected decimal ToBase(decimal amountInSourceCurrency, decimal sourceCurrencyExchangeRate)
        {
            return amountInSourceCurrency / sourceCurrencyExchangeRate;
        }

        /// <summary>
        ///  Formula for conversion: source to target currency.
        /// </summary>
        /// <param name="amountInSourceCurrency">Amount in source currency.</param>
        /// <param name="sourceCurrencyExchangeRate">Source currency exchange rate.</param>
        /// <param name="targetCurrencyExchangeRate">Target currency exchange rate.</param>
        /// <returns>Returns amount converted from source to target currency using cross-rate of base currency.</returns>
        protected decimal FromSourceToTarget(decimal amountInSourceCurrency, decimal sourceCurrencyExchangeRate, decimal targetCurrencyExchangeRate)
        {
            var sourceAmountInBaseCurrency =
                this.ToBase(amountInSourceCurrency, sourceCurrencyExchangeRate);

            return
                this.FromBase(sourceAmountInBaseCurrency, targetCurrencyExchangeRate);
        }

        #endregion private functions



        #region convert by ISO code
        /// <summary>
        /// Returns amount converted from Source Currency to Base Currency.
        /// </summary>
        /// <param name="amount">Amount in source currency.</param>
        /// <param name="sourceCurrencyIsoCode">Source Currency ISO code.</param>
        /// <returns>Returns amount converted from Source Currency to Base Currency.</returns>
        public decimal ToBaseCurrency(decimal amount, string sourceCurrencyIsoCode)
        {
            return this.FromSourceCurrencyToTargetCurrency(
                amount, sourceCurrencyIsoCode, this.BaseCurrency.ISOCurrencyCode);
        }

        public decimal FromBaseCurrency(decimal amountInBaseCurrency, string targetCurrencyIsoCode)
        {
            return this.FromSourceCurrencyToTargetCurrency(
                amountInBaseCurrency, this.BaseCurrency.ISOCurrencyCode, targetCurrencyIsoCode);
        }

        public decimal FromSourceCurrencyToTargetCurrency(
            decimal sourceCurrencyAmount, string sourceCurrencyIsoCode, string targetCurrencyIsoCode)
        {
            if (sourceCurrencyAmount == 0)
            {
                return 0;
            }

            var sourceCurrencyExchangeRate = this.GetExhangeRate(sourceCurrencyIsoCode);

            var targetCurrencyExchangeRate = this.GetExhangeRate(targetCurrencyIsoCode);


            var targetCurrencyAmount =
                this.FromSourceToTarget(
                sourceCurrencyAmount, sourceCurrencyExchangeRate, targetCurrencyExchangeRate);

            return targetCurrencyAmount;
        }
        #endregion convert by ISO code



        #region convert by Currency Id code
        public decimal ToBaseCurrency(decimal amount, Guid sourceCurrencyId)
        {
            return this.FromSourceCurrencyToTargetCurrency(
                amount, sourceCurrencyId, this.BaseCurrency.TransactionCurrencyId.Value);
        }

        public decimal FromBaseCurrency(decimal amountInBaseCurrency, Guid targetCurrencyId)
        {
            return this.FromSourceCurrencyToTargetCurrency(
                amountInBaseCurrency, this.BaseCurrency.TransactionCurrencyId.Value, targetCurrencyId);
        }

        public decimal FromSourceCurrencyToTargetCurrency(
            decimal sourceCurrencyAmount, Guid sourceCurrencyId, Guid targetCurrencyId)
        {
            var sourceCurrencyISOCode = this.GetCurrency(sourceCurrencyId).ISOCurrencyCode;

            var targetCurrencyISOCode = this.GetCurrency(targetCurrencyId).ISOCurrencyCode;

            var targetCurrencyAmount =
                this.FromSourceCurrencyToTargetCurrency(
                    sourceCurrencyAmount, sourceCurrencyISOCode, targetCurrencyISOCode);

            return targetCurrencyAmount;
        }

        #endregion convert by Currency Id code
    }
}
