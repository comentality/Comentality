using Microsoft.Xrm.Sdk;
using System;
using Comentality.content.Comentality;

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

        /// <summary>
        /// String representation of image type.
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        public static string Readable(this ImageType it)
        {
            if (it == ImageType.PreImage)
            {
                return "PreImage";
            }
            if (it == ImageType.PostImage)
            {
                return "PostImage";
            }
            throw new InvalidPluginExecutionException("Comentality.Extensions.Readable failed.");
        }
    }
}
