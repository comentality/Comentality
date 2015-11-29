using Microsoft.Xrm.Sdk;
using System;

namespace Comentality
{
    public static class Extensions
    {
        // TODO: move to context builder
        /// <summary>
        /// Converts enum value to OptionSetValue.
        /// </summary>
        public static OptionSetValue ToOptionSetValue(this Enum e)
        {
            return new OptionSetValue(Convert.ToInt32(e));
        }

        public static decimal ValueOrZero(this Money m)
        {
            return m?.Value ?? 0;
        }
    }
}
