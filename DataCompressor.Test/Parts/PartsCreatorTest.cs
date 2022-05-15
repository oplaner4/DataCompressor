using DataCompressor.Parts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class PartsCreatorTest
    {
        private readonly NodePart[] DirToCompressNodeParts;

        public PartsCreatorTest()
        {
            ModesPart modes = new ModesPart().SetOn(Mode.FileTreeMode);

            DirToCompressNodeParts = new NodePart[]
            {
                new NodePart("guide.txt", modes, 0),
                new NodePart("chicago.jpg", modes, 5),
                new NodePart("flowers.jpg", modes, 6),
                new NodePart("sea.jpg", modes, 5),

                new NodePart("Empty", modes, 4),
                new NodePart("Media", modes, 5),
                new NodePart("Interesting", modes, 5),
            };
        }

        [Fact]
        public async Task CreateEmpty()
        {
            PartsCreator creator = new(
                $"{TestData.DirToCompress}/Empty", new ModesPart());
            await creator.Prepare();

            IEnumerable<NodePart> parts = creator.GetNodeParts();
            Assert.False(parts.Any());
        }

        [Fact]
        public async Task Create()
        {
            PartsCreator creator = new(TestData.DirToCompress, new ModesPart());

            bool success = await creator.Prepare();
            Assert.True(success);

            IEnumerable<NodePart> parts = creator.GetNodeParts();

            long count = 0;
            foreach (NodePart part in parts)
            {
                Assert.Equal(DirToCompressNodeParts[count].Name, part.Name);
                Assert.Equal(DirToCompressNodeParts[count].ParentIndex.Value,
                    part.ParentIndex.Value);

                count++;
            }

            Assert.Equal(DirToCompressNodeParts.LongLength, count);
            Assert.Equal(creator.NodesCount, count);
        }
    }
}