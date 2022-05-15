using System;

namespace DataCompressor.Exceptions
{
    /// <summary>
    /// Exception to handle invalid data provided by user  
    /// to construct objects from bytes.
    /// </summary>
    public class UnableToConstructException : Exception
    {
        public UnableToConstructException()
        {
        }

        public UnableToConstructException(string message)
            : base(message)
        {
        }

        public UnableToConstructException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
