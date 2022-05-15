using DataCompressor.Exceptions;
using System.Collections.Generic;

namespace DataCompressor.Helpers
{
    public class ConstructEnumerator
    {
        private readonly IEnumerator<byte> Enumerator;

        private bool AnotherByte;

        /// <summary>
        /// Activates construct enumerator to easily work with bytes.
        /// </summary>
        /// <param name="bytes">Bytes enumerable to use</param>
        public ConstructEnumerator(IEnumerable<byte> bytes)
        {
            Enumerator = bytes.GetEnumerator();
            MoveNext();
        }

        /// <summary>
        /// Tries to move to the next byte with additional options.
        /// </summary>
        /// <param name="requireNext">Another byte is expected</param>
        /// <param name="failMessage">Exception message to use when next byte
        /// was required but not provided.</param>
        /// <returns>Successfully moved</returns>
        /// <exception cref="UnableToConstructException">This exception is thrown
        /// when another byte was expected but not provided.</exception>
        public bool MoveNext(bool requireNext = false, string failMessage = "")
        {
            AnotherByte = Enumerator.MoveNext();

            if (AnotherByte)
            {
                return true;
            }

            if (requireNext)
            {
                throw new UnableToConstructException(failMessage);
            }

            return false;
        }

        /// <summary>
        /// Tries to get current byte.
        /// </summary>
        /// <param name="failMessage">Exception message to use</param>
        /// <returns>current byte</returns>
        /// <exception cref="UnableToConstructException">Thrown
        /// when trying to get current byte but enumeration has finished.
        /// </exception>
        public byte GetCurrent(
            string failMessage = "Enumeration has finished")
        {
            if (AnotherByte)
            {
                return Enumerator.Current;
            }

            throw new UnableToConstructException(failMessage);
        }


        /// <summary>
        /// Tries to get more bytes at once and moves enumerator.
        /// </summary>
        /// <param name="count">Count of bytes to get</param>
        /// <param name="failMessage"></param>
        /// <returns>Byte array</returns>
        public byte[] GetMore(int count,
            string failMessage = "Lack of bytes")
        {
            byte[] result = new byte[count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetCurrent();
                MoveNext(i < result.Length - 1, failMessage);
            }

            return result;
        }
    }
}
