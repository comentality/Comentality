using System;
using System.Runtime.Serialization;

namespace Comentality.Exceptions
{
    [Serializable]
    public class NoTargetException : Exception
    {
        public NoTargetException() : base() { }

        public NoTargetException(string message) : base(message) { }

        public NoTargetException(string message, Exception inner) : base(message, inner) { }

        public NoTargetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
