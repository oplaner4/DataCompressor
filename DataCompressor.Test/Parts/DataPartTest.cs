using DataCompressor.Helpers;
using DataCompressor.Parts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class DataPartTest
    {
        private readonly IEnumerable<byte[]> TestData;
        private readonly uint TestDataCount;
        private readonly byte[] TestDataBytesWithRepeatMode;
        private readonly byte[] TestDataBytesWithoutRepeatMode;

        public DataPartTest()
        {
            List<byte[]> testData = new()
            {
                Array.Empty<byte>(),
                Encoding.UTF8.GetBytes("soon"),
                Encoding.UTF8.GetBytes("x"),
                Encoding.UTF8.GetBytes("health"),
                Encoding.UTF8.GetBytes("oop programming"),
                Encoding.UTF8.GetBytes("facebook"),
                Encoding.UTF8.GetBytes("employee"),
            };

            TestData = testData;
            TestDataCount = (uint)testData.Count;

            TestDataBytesWithRepeatMode = new byte[] {
               0, 0, 0, (byte)TestDataCount,
               0, 0, 0, 0,

               0, 0, 0, 3,
               (byte)'s', 0, 0, 0, 1,
               (byte)'o', 0, 0, 0, 2,
               (byte)'n', 0, 0, 0, 1,

               0, 0, 0, 1,
               (byte)'x', 0, 0, 0, 1,

               0, 0, 0, 6,
               (byte)'h', 0, 0, 0, 1,
               (byte)'e', 0, 0, 0, 1,
               (byte)'a', 0, 0, 0, 1,
               (byte)'l', 0, 0, 0, 1,
               (byte)'t', 0, 0, 0, 1,
               (byte)'h', 0, 0, 0, 1,

               0, 0, 0, 13,
               (byte)'o', 0, 0, 0, 2,
               (byte)'p', 0, 0, 0, 1,
               (byte)' ', 0, 0, 0, 1,
               (byte)'p', 0, 0, 0, 1,
               (byte)'r', 0, 0, 0, 1,
               (byte)'o', 0, 0, 0, 1,
               (byte)'g', 0, 0, 0, 1,
               (byte)'r', 0, 0, 0, 1,
               (byte)'a', 0, 0, 0, 1,
               (byte)'m', 0, 0, 0, 2,
               (byte)'i', 0, 0, 0, 1,
               (byte)'n', 0, 0, 0, 1,
               (byte)'g', 0, 0, 0, 1,

               0, 0, 0, 7,
               (byte)'f', 0, 0, 0, 1,
               (byte)'a', 0, 0, 0, 1,
               (byte)'c', 0, 0, 0, 1,
               (byte)'e', 0, 0, 0, 1,
               (byte)'b', 0, 0, 0, 1,
               (byte)'o', 0, 0, 0, 2,
               (byte)'k', 0, 0, 0, 1,

               0, 0, 0, 7,
               (byte)'e', 0, 0, 0, 1,
               (byte)'m', 0, 0, 0, 1,
               (byte)'p', 0, 0, 0, 1,
               (byte)'l', 0, 0, 0, 1,
               (byte)'o', 0, 0, 0, 1,
               (byte)'y', 0, 0, 0, 1,
               (byte)'e', 0, 0, 0, 2,
            };

            TestDataBytesWithoutRepeatMode = new byte[] {
               0, 0, 0, (byte)TestDataCount,
               0, 0, 0, 0,

               0, 0, 0, 4,
               (byte)'s',
               (byte)'o',
               (byte)'o',
               (byte)'n',

               0, 0, 0, 1,
               (byte)'x',

               0, 0, 0, 6,
               (byte)'h',
               (byte)'e',
               (byte)'a',
               (byte)'l',
               (byte)'t',
               (byte)'h',

               0, 0, 0, 15,
               (byte)'o',
               (byte)'o',
               (byte)'p',
               (byte)' ',
               (byte)'p',
               (byte)'r',
               (byte)'o',
               (byte)'g',
               (byte)'r',
               (byte)'a',
               (byte)'m',
               (byte)'m',
               (byte)'i',
               (byte)'n',
               (byte)'g',

               0, 0, 0, 8,
               (byte)'f',
               (byte)'a',
               (byte)'c',
               (byte)'e',
               (byte)'b',
               (byte)'o',
               (byte)'o',
               (byte)'k',

               0, 0, 0, 8,
               (byte)'e',
               (byte)'m',
               (byte)'p',
               (byte)'l',
               (byte)'o',
               (byte)'y',
               (byte)'e',
               (byte)'e',
            };
        }

        [Fact]
        public void CreateData_UseRepeatMode_ShouldSucceed()
        {
            ModesPart modes = new();

            DataPart manager = new(
                TestData,
                TestDataCount,
                modes
            );

            Assert.True(manager.PrepareBytes());
            Assert.Equal(TestDataBytesWithRepeatMode, manager.GetBytes());
            Assert.Equal(TestDataCount, manager.Count.Value);
        }

        [Fact]
        public void CreateData_WithoutRepeatMode_ShouldSucceed()
        {
            ModesPart modes = new ModesPart().SetOff(Mode.RepeatMode);

            DataPart manager = new(
                TestData,
                TestDataCount,
                modes
            );

            Assert.True(manager.PrepareBytes());
            Assert.Equal(TestDataBytesWithoutRepeatMode, manager.GetBytes());
            Assert.Equal(TestDataCount, manager.Count.Value);
        }

        [Fact]
        public void ReconstructData_UseRepeatMode_ShouldSucceed()
        {
            ModesPart modes = new();

            DataPart manager = new(
                new ConstructEnumerator(TestDataBytesWithRepeatMode),
                modes
            );

            Assert.Equal(TestDataCount, manager.Count.Value);

            IEnumerator<byte[]> testDataEnumerator = TestData.GetEnumerator();
            testDataEnumerator.MoveNext();

            foreach (FilePart fileManager in manager.FilesParts)
            {
                Assert.Equal(testDataEnumerator.Current, fileManager.Fragments);
                testDataEnumerator.MoveNext();
            }
        }

        [Fact]
        public void ReconstructData_WithoutRepeatMode_ShouldSucceed()
        {
            ModesPart modes = new ModesPart().SetOff(Mode.RepeatMode);

            DataPart manager = new(
                new ConstructEnumerator(
                    TestDataBytesWithoutRepeatMode),
                modes
            );

            Assert.Equal(TestDataCount, manager.Count.Value);

            IEnumerator<byte[]> testDataEnumerator = TestData.GetEnumerator();
            testDataEnumerator.MoveNext();

            foreach (FilePart fileManager in manager.FilesParts)
            {
                FilePart testFileManager = new(testDataEnumerator.Current, modes);

                Assert.Equal(testFileManager.Fragments, fileManager.Fragments);
                testDataEnumerator.MoveNext();
            }
        }
    }
}