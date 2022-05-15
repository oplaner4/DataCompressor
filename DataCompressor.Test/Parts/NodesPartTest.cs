using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using DataCompressor.Parts;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class NodesPartTest
    {
        [Theory]
        [InlineData(
            0,
            new string[] { },
            new ulong[] { },
            false,
            new byte[] {
                0, 0, 0, 0,
            })
        ]
        [InlineData(
            0,
            new string[] { },
            new ulong[] { },
            true,
            new byte[] {
                0, 0, 0, 0,
            })
        ]
        [InlineData(
            1,
            new string[] { ".gitkeep" },
            new ulong[] { 0 },
            false,
            new byte[] {
                0, 0, 0, 1,
                8,
                (byte)'.', (byte)'g', (byte)'i', (byte)'t',
                (byte)'k', (byte)'e',(byte)'e', (byte)'p' })]
        [InlineData(
            1,
            new string[] { "README.md" },
            new ulong[] { 3_212_145_153 },
            true,
            new byte[] {
                0, 0, 0, 1,
                0b1011_1111, 0b0111_0101, 0b0111_0010, 0b0000_0001,
                9,
                (byte)'R', (byte)'E', (byte)'A', (byte)'D',
                (byte)'M', (byte)'E',(byte)'.', (byte)'m', (byte)'d' })]
        [InlineData(
            3,
            new string[] { "solution.txt", "code.py", "data.csv" },
            new ulong[] { 0, 0, 0 },
            false,
            new byte[] {
                0, 0, 0, 3,
                12,
                (byte)'s', (byte)'o', (byte)'l', (byte)'u',
                (byte)'t', (byte)'i',(byte)'o', (byte)'n',
                (byte)'.', (byte)'t', (byte)'x', (byte)'t',
                7,
                (byte)'c', (byte)'o', (byte)'d',
                (byte)'e', (byte)'.', (byte)'p', (byte)'y',
                8,
                (byte)'d', (byte)'a', (byte)'t',
                (byte)'a', (byte)'.', (byte)'c', (byte)'s',
                (byte)'v' })]
        [InlineData(
            2,
            new string[] { "public", "script.js" },
            new ulong[] { 0, 0 },
            true,
            new byte[] {
                0, 0, 0, 2,
                0, 0, 0, 0,
                6,
                (byte)'p', (byte)'u', (byte)'b',
                (byte)'l', (byte)'i', (byte)'c',
                0, 0, 0, 0,
                9,
                (byte)'s', (byte)'c', (byte)'r', (byte)'i', (byte)'p',
                (byte)'t', (byte)'.', (byte)'j', (byte)'s' })]
        [InlineData(
            4,
            new string[] { "index.js", "js", "css", "style.css" },
            new ulong[] { 1, 1, 2, 2 },
            true,
            new byte[] {
                0, 0, 0, 4,
                0, 0, 0, 1,
                8,
                (byte)'i', (byte)'n', (byte)'d',
                (byte)'e', (byte)'x', (byte)'.', (byte)'j', (byte)'s',
                0, 0, 0, 1,
                2,
                (byte)'j', (byte)'s',
                0, 0, 0, 2,
                3,
                (byte)'c', (byte)'s', (byte)'s',
                0, 0, 0, 2,
                9,
                (byte)'s', (byte)'t', (byte)'y', (byte)'l', (byte)'e',
                (byte)'.', (byte)'c', (byte)'s', (byte)'s' })]
        public void CreateNodes_ShouldSucceed(
            long nodesCount,
            string[] names, ulong[] parentIndices, bool fileTreeModeOn,
            byte[] expected)
        {
            ModesPart modes = new();

            if (fileTreeModeOn)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            NodePart[] nodes = new NodePart[nodesCount];

            for (long i = 0; i < nodesCount; i++)
            {
                nodes[i] = new(names[i], modes, parentIndices[i]);
            }

            NodesPart nodesManager = new(nodes, nodesCount, modes);
            Assert.Equal((ulong)nodesCount, nodesManager.Count.Value);
            Assert.Equal(expected, nodesManager.GetBytes());
        }



        [Theory]
        [InlineData(
            new byte[] {
                0, 0, 0, 0
            },
            true,
            new string[] { },
            new ulong[] { },
            0
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 0
            },
            false,
            new string[] { },
            new ulong[] { },
            0
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 1,
                9,
                (byte)'p', (byte)'h', (byte)'o', (byte)'t', (byte)'o',
                (byte)'.', (byte)'j', (byte)'p', (byte)'g'
            },
            false,
            new string[] { "photo.jpg" },
            new ulong[] { 0 },
            1
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 1,
                0b1001_0010, 0b0111_0110, 0b0001_1100, 0b1010_1001,
                8,
                (byte)'s', (byte)'o', (byte)'n', (byte)'g', (byte)'.',
                (byte)'m', (byte)'p', (byte)'3'
            },
            true,
            new string[] { "song.mp3" },
            new ulong[] { 2_457_214_121 },
            1
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 2,
                0, 0, 0, 1,
                9,
                (byte)'s', (byte)'c', (byte)'r', (byte)'i', (byte)'p',
                (byte)'t', (byte)'.', (byte)'j', (byte)'s',
                0, 0, 0, 1,
                6,
                (byte)'p', (byte)'u', (byte)'b',
                (byte)'l', (byte)'i', (byte)'c'
            },
            true,
            new string[] { "script.js", "public" },
            new ulong[] { 1, 1 },
            2
        )]
        [InlineData(
            new byte[] {
                0, 0, 0, 4,
                0, 0, 0, 2,
                8,
                (byte)'i', (byte)'n', (byte)'d',
                (byte)'e', (byte)'x', (byte)'.', (byte)'j', (byte)'s',
                0, 0, 0, 3,
                9,
                (byte)'s', (byte)'t', (byte)'y', (byte)'l', (byte)'e',
                (byte)'.', (byte)'c', (byte)'s', (byte)'s',
                0, 0, 0, 2,
                2,
                (byte)'j', (byte)'s',
                0, 0, 0, 3,
                3,
                (byte)'c', (byte)'s', (byte)'s'
            },
            true,
            new string[] { "index.js", "style.css", "js", "css" },
            new ulong[] { 2, 3, 2, 3 },
            4
        )]
        public void ReconstructNodes_ShouldSucceed(
            byte[] nodesBytes,
            bool fileTreeModeOn,
            string[] expectedNames,
            ulong[] expectedParentIndices,
            ulong expectedCount
        )
        {
            ModesPart modes = new();
            if (fileTreeModeOn)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            ConstructEnumerator bytesEnumerator = new(nodesBytes);

            NodesPart nodesManager = new(
                bytesEnumerator, modes);
            Assert.Equal(expectedCount, nodesManager.Count.Value);

            IEnumerator<NodePart> enumerator = nodesManager
                .NodesParts.GetEnumerator();
            enumerator.MoveNext();

            for (ulong i = 0; i < expectedCount; i++)
            {
                Assert.Equal(expectedNames[i], enumerator.Current.Name);
                if (fileTreeModeOn)
                {
                    Assert.Equal(expectedParentIndices[i],
                       enumerator.Current.ParentIndex.Value);
                }

                enumerator.MoveNext();
            }

            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.Throws<UnableToConstructException>(() =>
                bytesEnumerator.GetCurrent());
        }


        [Theory]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0 }, false)]
        [InlineData(new byte[] { 1 }, false)]
        [InlineData(new byte[] { 0, 0, 0 }, false)]
        [InlineData(new byte[] { 0, 0, 0, 1, 0, 0 }, true)]
        [InlineData(new byte[] {
            6,
            (byte)'s', (byte)'m', (byte)'t', (byte)'p'
        }, false)]
        [InlineData(new byte[] {
            1, 2, 8, 3,
            3,
            (byte)'h', (byte)'w', (byte)'1',
            9, 2, 7, 4,
            9,
            (byte)'s', (byte)'o', (byte)'l',(byte)'.',(byte)'t',(byte)'x',
            (byte)'t',
        }, true)]
        public void ReconstructNode_ShouldFail(IEnumerable<byte> bytes,
            bool fileTreeModeOn)
        {
            ModesPart modes = new();
            if (fileTreeModeOn)
            {
                modes.SetOn(Mode.FileTreeMode);
            }

            Assert.Throws<UnableToConstructException>(() =>
                new NodesPart(new(bytes), modes));
        }
    }
}