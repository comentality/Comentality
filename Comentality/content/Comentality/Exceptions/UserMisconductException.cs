using System;
using System.Runtime.Serialization;

namespace Comentality.Exceptions
{
    [Serializable]
    public class UserMisconductException : Exception
    {
        public UserMisconductException()
        {
        }

        public UserMisconductException(string message) : base(message)
        {
        }

        public UserMisconductException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UserMisconductException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
