using System;
using System.Runtime.Serialization;

namespace Comentality.Exceptions
{
    [Serializable]
    public class NoImageException : Exception
    {
        public NoImageException() : base() { }

        public NoImageException(string message) : base(message) { }

        public NoImageException(string message, Exception inner) : base(message, inner) { }

        public NoImageException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        //TODO(commentality): comentality, replace image type with enum, replace Target name with const.
        internal static string BuildMessage(string imageName, int imageType)
        {
            string message;

            if (imageName == "Target")
            {
                message = string.Format(
                    "Failed to read {0} 'Target'. " +
                    "'Target' is default name for an image. " +
                    "If you want to use other name -- you should pass it as argument PluginHelper.GetPreImage<T>(\"image_name\"). " +
                    "Doublecheck that 'Target' image is registered, has.",
                     imageType);
            }
            else
            {
                message = string.Format(
                    "Failed to read {0} '{1}'. " +
                    "Doublecheck that '{1}' image is registered, is {0}, has this exact name. ",
                     imageType, imageName);
            }

            return message;
        }
    }
}
