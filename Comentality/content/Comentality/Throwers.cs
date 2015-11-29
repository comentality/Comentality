namespace Comentality
{
    using System;
    using Exceptions;
    using Microsoft.Xrm.Sdk;

    public class Throwers
    {
        public static void ThrowOnWrongReferenceType(EntityReference id, string typeName)
        {
            if (id.LogicalName != typeName)
            {
                var message =
                    $"EntityReference of type {id.LogicalName} was used in the context where {typeName} is expected.";

                throw new WrongEntityReferenceTypeException(message);
            }
        }

        public static void ThrowOnNullArgument(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ThrowOnNullOrEmptyArgument(string argument, string argumentName)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
