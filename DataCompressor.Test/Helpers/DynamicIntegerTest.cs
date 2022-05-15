using DataCompressor.Helpers;
using DataCompressor.Parts;
using Xunit;

namespace DataCompressor.Test.Helpers
{
    public class DynamicIntegerTest
    {
        [Theory]
        [InlineData(0, 1)]
        [InlineData(40, 1)]
        [InlineData(0b1111_1111, 1)]
        [InlineData(0, 2)]
        [InlineData(1218, 2)]
        [InlineData(0b1_0000_0000, 2)]
        [InlineData(0b1111_1111_1111_1111, 2)]
        [InlineData(0b1_0000_0000_0000_0000, 4)]
        [InlineData(2_121_596_456, 4)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111, 4)]
        [InlineData(0, 8)]
        [InlineData(0b1_1111_1111_1111_1111_1111_1111_1111_1111, 8)]
        [InlineData(4_125_453_984_514_145, 8)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111, 8)]
        public void EnoughBytes_ShouldSucceed(
            ulong value, int bytesCount)
        {
            DynamicInteger count = new(bytesCount);
            Assert.True(count.Set(value));
            Assert.Equal(value, count.Value);
            Assert.Equal(value, count.Get());

            DynamicInteger countByBytes = new(bytesCount, count.Bytes);
            Assert.True(countByBytes.Set(value));
            Assert.Equal(value, countByBytes.Value);
            Assert.Equal(value, countByBytes.Get());
        }

        [Theory]
        [InlineData(0b1111_1111_1111_1111, 1)]
        [InlineData(0b1_0000_0000_0000_0000, 2)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111, 4)]
        public void NotEnoughBytes_ShouldFail(
            ulong value, int bytesCount)
        {
            DynamicInteger count = new(bytesCount);
            Assert.False(count.Set(value));
            Assert.Equal((ulong)0, count.Get());
            Assert.Equal((ulong)0, count.Value);
        }

        [Theory]
        [InlineData(0b1111_1111, 1, 1, false)]
        [InlineData(0b1111_1111, 1, 2, true)]
        [InlineData(0b1111_1111_1101_1111, 1, 2, true)]
        [InlineData(0b1111_1111_1111_1101, 3, 2, false)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1110,
            1, 8, true)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111,
            1, 8, false)]
        public void Addition_ShouldSucceed(
            ulong value, ulong increment, int countBytes, bool expectedSuccess)
        {
            DynamicInteger integer = new(countBytes);
            Assert.True(integer.Set(value));
            Assert.Equal(expectedSuccess, integer.Add(increment));
        }

        [Theory]
        [InlineData(0, false, false)]
        [InlineData(40, false, false)]
        [InlineData(0b1111_1111, false, false)]
        [InlineData(0, true, false)]
        [InlineData(0b1111_0000_1111_0000, true, false)]
        [InlineData(0b1111_1111_1111_1111, true, false)]
        [InlineData(0, false, true)]
        [InlineData(0b1_1111_1111_1111_1111, false, true)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111, false, true)]
        [InlineData(0, true, true)]
        [InlineData(0b1_1111_1111_1111_1111_1111_1111_1111_1111, true, true)]
        [InlineData(0b1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111, true, true)]
        public void EnoughBytesModes_ShouldSucceed(
            ulong value, bool shortModeOn, bool intModeOn)
        {
            DynamicInteger count = new(
                ModesPart.GetBytesCount(shortModeOn, intModeOn));

            Assert.True(count.Set(value));
            Assert.Equal(value, count.Get());
            Assert.Equal(value, count.Value);
        }

        [Theory]
        [InlineData(1, byte.MaxValue)]
        [InlineData(2, ushort.MaxValue)]
        [InlineData(4, uint.MaxValue)]
        [InlineData(8, ulong.MaxValue)]
        public void GetMaxValue_ShouldSucceed(
            int bytesCount, ulong expectedValue)
        {
            DynamicInteger count = new(bytesCount);
            Assert.Equal(expectedValue, count.GetMaxValue());
        }
    }
}