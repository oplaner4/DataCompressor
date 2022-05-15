using System.Collections.Generic;

namespace DataCompressor.Helpers
{
    public class DynamicInteger
    {
        private static readonly int BitsInByte = 8;

        private readonly int BytesCount;

        /// <summary>
        /// Value of integer. Zero by default.
        /// </summary>
        public ulong Value { get; private set; }

        /// <summary>
        /// Bytes representing integer in a big-endian order.
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <param name="bytesCount">Count of bytes to use from interval [0, 8]
        /// </param>
        /// <param name="bytes">Bytes in big-endian order to create from</param>
        public DynamicInteger(int bytesCount, byte[]? bytes = null)
        {
            BytesCount = bytesCount;
            Bytes = bytes ?? new byte[BytesCount];

            if (bytes != null)
            {
                Value = Get();
            }
        }

        /// <summary>
        /// Tries to set value with defined count of bytes.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <returns>True if successfully set using defined
        /// bytes count. False indicates that count of bytes is too
        /// small and should be raised.</returns>
        public bool Set(ulong value)
        {
            Stack<byte> stack = new();

            for (int i = 0; i < sizeof(ulong); i++)
            {
                ulong b = value >> (i * BitsInByte) & byte.MaxValue;
                stack.Push((byte)b);
            }

            while (stack.Peek() == 0 && stack.Count > BytesCount)
            {
                stack.Pop();
            }

            if (stack.Count > BytesCount)
            {
                return false;
            }

            Bytes = stack.ToArray();
            Value = value;
            return true;
        }

        /// <summary>
        /// Adds value and checks for overflow.
        /// </summary>
        public bool Add(ulong increment)
        {
            if (increment > (GetMaxValue() - Value))
            {
                return false;
            }

            Set(Value + increment);
            return true;
        }

        /// <summary>
        /// Gets max value acceptable for set bytes count
        /// </summary>
        /// <returns>Ulong value</returns>
        public ulong GetMaxValue()
        {
            if (BytesCount == sizeof(ulong))
            {
                return ulong.MaxValue;
            }

            ulong result = 0;
            int shift = 0;

            for (int i = BytesCount - 1; i >= 0; i--)
            {
                result += (ulong)byte.MaxValue << shift;
                shift += BitsInByte;
            }

            return result;
        }

        public ulong Get()
        {
            ulong result = 0;
            int shift = 0;

            for (int i = BytesCount - 1; i >= 0; i--)
            {
                result += (ulong)Bytes[i] << shift;
                shift += BitsInByte;
            }

            return result;
        }
    }
}
