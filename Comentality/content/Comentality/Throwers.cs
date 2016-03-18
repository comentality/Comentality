namespace Comentality
{
    using System;
    using Microsoft.Xrm.Sdk;

    public static class Throwers
    {
        public static void IfReferenceTypeIsWrong(EntityReference id, string typeName)
        {
            if (id.LogicalName != typeName)
            {
                throw new InvalidPluginExecutionException(
                    $"EntityReference of type {id.LogicalName} was used in the context where {typeName} is expected.");
            }
        }

        public static void IfNullArgument(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void IfNullOrEmptyArgument(string argument, string argumentName)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void UserMisconduct(string format, params object[] args)
        {
            throw new InvalidPluginExecutionException(string.Format("UserMisconductException: " + format, args));
        }

        public static void UnexpectedNullValue(string format, params object[] args)
        {
            throw new InvalidPluginExecutionException(string.Format("UnexpectedNullValueException: " + format, args));
        }
    }
}
