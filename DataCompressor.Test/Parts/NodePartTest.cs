using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using DataCompressor.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class NodePartTest
    {
        [Theory]
        [InlineData(new byte[] {
            1,
            (byte)'g'
        }, "g", null)]
        [InlineData(new byte[] {
            0, 0, 0, 0,
            1,
            (byte)'t'
        }, "t", 0UL)]
        [InlineData(new byte[] {
            9,
            (byte)'s', (byte)'c', (byte)'r', (byte)'i', (byte)'p',
            (byte)'t', (byte)'.', (byte)'j', (byte)'s'
        }, "script.js", null)]
        [InlineData(new byte[] {
            0, 0, 0, 0,
            7,
            (byte)'s', (byte)'o', (byte)'l', (byte)'.',
            (byte)'t', (byte)'x', (byte)'t'
        }, "sol.txt", 0UL)]
        [InlineData(new byte[] {
            0, 0, 0, 0b0100_1110,
            11,
            (byte)'d', (byte)'a', (byte)'t', (byte)'a',
            (byte)'s', (byte)'e', (byte)'t',
            (byte)'.',
            (byte)'c', (byte)'s', (byte)'v'
        }, "dataset.csv", 78UL)]
        [InlineData(new byte[] {
            0b1010_1011, 0b1010_1010, 0b1010_1111, 0b0110_1011,
            6,
            (byte)'o', (byte)'k', (byte)'.', (byte)'s', (byte)'v', (byte)'g'
        }, "ok.svg", 2880089963UL)]
        public void CreateNode_ShouldSucceed(
            byte[] expected, string name, ulong? parentIndex)
        {
            ModesPart modes = new();
            if (parentIndex.HasValue)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            NodePart manager = new(name, modes, parentIndex);

            Assert.Equal(expected, manager.GetBytes().ToArray());
            Assert.Equal(name.Length, manager.Len);
        }

        [Fact]
        public void CreateNodes_ShouldFail()
        {
            Assert.Throws<ArgumentNullException>(
                () => new NodePart("", new ModesPart()));
        }

        [Theory]
        [InlineData(new byte[] {
            1,
            (byte)'h'
        }, "h", null)]
        [InlineData(new byte[] {
            0, 0b0100_0101, 0b1000_0110, 0b0100_0101,
            1,
            (byte)'p'
        }, "p", 4_556_357UL)]
        [InlineData(new byte[] {
            9,
            (byte)'s', (byte)'c', (byte)'r', (byte)'i', (byte)'p',
            (byte)'t', (byte)'.', (byte)'j', (byte)'s'
        }, "script.js", null)]
        [InlineData(new byte[] {
            0, 0, 0, 0,
            7,
            (byte)'s', (byte)'o', (byte)'l', (byte)'.', (byte)'t', (byte)'x', (byte)'t'
        }, "sol.txt", 0UL)]
        [InlineData(new byte[] {
            0, 0, 0, 0b0100_1110,
            11,
            (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)'s', (byte)'e', (byte)'t',
            (byte)'.', (byte)'c', (byte)'s', (byte)'v'
        }, "dataset.csv", 78UL)]
        [InlineData(new byte[] {
            0b1010_1011, 0b1010_1010, 0b1010_1111, 0b0110_1011,
            6,
            (byte)'o', (byte)'k', (byte)'.', (byte)'s', (byte)'v', (byte)'g'
        }, "ok.svg", 2880089963UL)]
        public void ReconstructNode_ShouldSucceed(IEnumerable<byte> nodeBytes,
            string expectedName, ulong? expectedParentIndex)
        {
            ConstructEnumerator enumerator = new(nodeBytes);

            ModesPart modes = new();
            if (expectedParentIndex != null)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            NodePart manager = new(enumerator, modes);
            Assert.Equal(expectedName, manager.Name);
            Assert.Equal(expectedName.Length, manager.Len);

            if (modes.IsOn(Mode.FileTreeMode))
            {
                Assert.Equal(expectedParentIndex, manager.ParentIndex.Value);
            }

            Assert.Throws<UnableToConstructException>(() =>
                enumerator.MoveNext(true, "Should fail"));
        }


        [Theory]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0 }, false)]
        [InlineData(new byte[] { 1 }, false)]
        [InlineData(new byte[] { 0, 0, 0 }, false)]
        [InlineData(new byte[] {
            6,
            (byte)'s', (byte)'m', (byte)'t', (byte)'p'
        }, false)]
        [InlineData(new byte[] {
            3, 0, 0,
            1,
            (byte)'x'
        }, true)]
        [InlineData(new byte[] {
            3, 0, 0, 0,
            2,
            (byte)'r'
        }, true)]
        public void ReconstructNode_ShouldFail(IEnumerable<byte> nodeBytes,
            bool fileTreeModeOn)
        {
            ModesPart modes = new();

            if (fileTreeModeOn)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            Assert.Throws<UnableToConstructException>(() =>
                new NodePart(new ConstructEnumerator(nodeBytes), modes));
        }

        [Fact]
        public void CreateNodeInvalidParent_ShouldFail()
        {
            Assert.Throws<UnableToConstructException>(() =>
                new NodePart("index.js",
                    new ModesPart().SetOn(Mode.NodesShortMode)
                        .SetOff(Mode.NodesIntMode),
                    0b1001_1111_1010_1111_1011));
        }
    }
}