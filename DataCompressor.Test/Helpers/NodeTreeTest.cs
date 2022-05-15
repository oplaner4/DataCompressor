using DataCompressor.Helpers;
using DataCompressor.Parts;
using System;
using System.Collections.Generic;
using Xunit;

namespace DataCompressor.Test.Helpers
{
    public class NodeTreeTest
    {
        [Fact]
        public void LookupEmpty()
        {
            NodeTree tree = new(Array.Empty<NodePart>(), 0);
            tree.Lookup((path, isDir, inx) =>
                throw new Exception());
        }

        [Fact]
        public void LookupBasic()
        {
            ModesPart modes = new();
            List<NodePart> nodes = new()
            {
                new NodePart("preview.jpg", modes),
                new NodePart("poster.jpg", modes),
                new NodePart("track01.mp3", modes),
                new NodePart("track02.mp3", modes),
            };

            NodeTree tree = new(nodes, (uint)nodes.Count);
            tree.Lookup((fullName, isDir, inx) =>
            {
                Assert.Equal("/" + nodes[(int)inx].Name, fullName);
            });
        }

        [Fact]
        public void LookupComplex()
        {
            ModesPart modes = new ModesPart()
                .SetOn(Mode.FileTreeMode);

            List<NodePart> nodes = new()
            {
                // files at first

                /* 0  */
                new NodePart("Settings.cs", modes, 21),
                /* 1  */
                new NodePart("NodeTree.cs", modes, 21),
                /* 2  */
                new NodePart("Mode.cs", modes, 21),
                /* 3  */
                new NodePart("ConstructEnumerator.cs", modes, 21),

                /* 4  */
                new NodePart("DataPart.cs", modes, 18),
                /* 5  */
                new NodePart("FilePart.cs", modes, 18),
                /* 6  */
                new NodePart("ModesPart.cs", modes, 18),
                /* 7  */
                new NodePart("NodePart.cs", modes, 18),
                /* 8  */
                new NodePart("NodesPart.cs", modes, 18),

                /* 9  */
                new NodePart("DataPartTest.cs", modes, 19),
                /* 10 */
                new NodePart("FilePartTest.cs", modes, 19),
                /* 11 */
                new NodePart("ModesPartTest.cs", modes, 19),
                /* 12 */
                new NodePart("NodePartTest.cs", modes, 19),
                /* 13 */
                new NodePart("NodesPartTest.cs", modes, 19),

                /* 14 */
                new NodePart("NodeTreeTest.cs", modes, 22),
                /* 15 */
                new NodePart("ConstructEnumeratorTest.cs", modes, 22),

                /* 16 */
                new NodePart("Empty.txt", modes, 16),

                /* 17 */
                new NodePart("UnableToConstructException.cs", modes, 20),

                // directories

                /* 18 */
                new NodePart("Parts", modes, 21),
                /* 19 */
                new NodePart("Parts", modes, 22),
                /* 20 */
                new NodePart("Exceptions", modes, 21),
                /* 21 */
                new NodePart("DataCompressor", modes, 21),
                /* 22 */
                new NodePart("DataCompressor.Test", modes, 22),
                /* 23 */
                new NodePart("Empty", modes, 18),
            };

            List<string> expectedNodeNamesWithPath = new()
            {
                "/DataCompressor/Settings.cs",
                "/DataCompressor/NodeTree.cs",
                "/DataCompressor/Mode.cs",
                "/DataCompressor/ConstructEnumerator.cs",

                "/DataCompressor/Parts/DataPart.cs",
                "/DataCompressor/Parts/FilePart.cs",
                "/DataCompressor/Parts/ModesPart.cs",
                "/DataCompressor/Parts/NodePart.cs",
                "/DataCompressor/Parts/NodesPart.cs",

                "/DataCompressor.Test/Parts/DataPartTest.cs",
                "/DataCompressor.Test/Parts/FilePartTest.cs",
                "/DataCompressor.Test/Parts/ModesPartTest.cs",
                "/DataCompressor.Test/Parts/NodePartTest.cs",
                "/DataCompressor.Test/Parts/NodesPartTest.cs",

                "/DataCompressor.Test/NodeTreeTest.cs",
                "/DataCompressor.Test/ConstructEnumeratorTest.cs",

                "/Empty.txt",
                "/DataCompressor/Exceptions/UnableToConstructException.cs",

                "/DataCompressor/Parts",
                "/DataCompressor.Test/Parts",
                "/DataCompressor/Exceptions",
                "/DataCompressor",
                "/DataCompressor.Test",
                "/DataCompressor/Parts/Empty",
            };

            NodeTree tree = new(nodes, 18);

            int count = 0;

            tree.Lookup((fullName, isDir, inx) =>
            {
                Assert.Equal(fullName,
                    expectedNodeNamesWithPath[(int)inx]);
                count++;
            });

            Assert.Equal(expectedNodeNamesWithPath.Count, count);
        }
    }
}