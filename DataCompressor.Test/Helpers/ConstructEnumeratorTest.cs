using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using System;
using System.Linq;
using Xunit;

namespace DataCompressor.Test.Helpers
{
    public class ConstructEnumeratorTest
    {
        [Fact]
        public void EnumerateEmpty_ShouldFail()
        {
            ConstructEnumerator enumerator = new(Array.Empty<byte>());
            Assert.Throws<UnableToConstructException>(() =>
                enumerator.GetCurrent("Should fail"));
        }

        [Fact]
        public void Enumerate_ShouldSucceed()
        {
            byte[] testBytes = new byte[] { 0, 1, 2, 3 };

            ConstructEnumerator enumerator = new(testBytes);
            Assert.True(enumerator.GetCurrent() == testBytes[0]);
            Assert.True(enumerator.MoveNext(true, "Should not fail."));
            Assert.True(enumerator.GetCurrent() == testBytes[1]);
            Assert.True(enumerator.MoveNext(true, "Should not fail."));
            Assert.True(enumerator.GetCurrent() == testBytes[2]);
            Assert.True(enumerator.MoveNext(true, "Should not fail."));
            Assert.True(enumerator.GetCurrent() == testBytes[3]);
            Assert.False(enumerator.MoveNext(false, "Should not fail."));
        }

        [Fact]
        public void GetMoreExact_ShouldSucceed()
        {
            byte[] testBytes = new byte[] { 2, 7, 4, 1 };

            ConstructEnumerator enumerator = new(testBytes);
            Assert.Equal(testBytes, enumerator.GetMore(testBytes.Length));
            Assert.Throws<UnableToConstructException>(() =>
                enumerator.GetCurrent("Should fail now"));
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void GetMore_ShouldSucceed()
        {
            byte[] testBytes = new byte[] { 3, 1, 9, 2, 17, 4 };
            int take = 5;

            ConstructEnumerator enumerator = new(testBytes);
            Assert.Equal(testBytes.Take(take), enumerator.GetMore(take));
            Assert.Equal(testBytes[take],
                enumerator.GetCurrent("Should not fail now"));
            Assert.False(enumerator.MoveNext());
            Assert.Throws<UnableToConstructException>(() =>
               enumerator.GetCurrent("Should fail now"));
        }

        [Fact]
        public void GetMore_ShouldFail()
        {
            byte[] testBytes = new byte[] { 3, 6, 1 };
            int take = 7;

            ConstructEnumerator enumerator = new(testBytes);
            Assert.Throws<UnableToConstructException>(() =>
                enumerator.GetMore(take, "Should fail"));
        }

        [Fact]
        public void EnumerateByKnownCount_ShouldFail()
        {
            int wantedCount = 4;
            byte[] testBytes = new byte[] { 7, 8, 2, /* missing */ };

            ConstructEnumerator enumerator = new(testBytes);

            for (int i = 0; i < wantedCount; i++)
            {
                if (i >= testBytes.Length - 1)
                {
                    Assert.Throws<UnableToConstructException>(
                        () => enumerator.MoveNext(true,
                            "Should fail. Another one wanted."));

                    Assert.Throws<UnableToConstructException>(
                        () => enumerator.GetCurrent(
                            "Should fail. Another one wanted."));
                }
                else
                {
                    Assert.True(enumerator.GetCurrent() == testBytes[i]);
                    Assert.True(enumerator.MoveNext(true, "Should not fail."));
                }
            }
        }
    }
}