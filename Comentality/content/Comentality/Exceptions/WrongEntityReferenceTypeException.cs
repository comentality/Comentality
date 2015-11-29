namespace Comentality.Exceptions
{
    using System;

    [Serializable]
    public class WrongEntityReferenceTypeException : Exception
    {
        public WrongEntityReferenceTypeException() { }
        public WrongEntityReferenceTypeException(string message) : base(message) { }
        public WrongEntityReferenceTypeException(string message, Exception inner) : base(message, inner) { }
        protected WrongEntityReferenceTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
