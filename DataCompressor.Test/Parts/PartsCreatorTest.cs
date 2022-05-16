using DataCompressor.Helpers;
using DataCompressor.Parts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataCompressor.Test.Parts
{
    public class PartsCreatorTest
    {
        [Fact]
        public async Task CreateEmpty()
        {
            string dirName = $"{TestData.DataDir}/PartsCreatorEmpty";
            Directory.CreateDirectory(dirName);
            PartsCreator creator = new(dirName, new ModesPart());
            Assert.True(await creator.Prepare());

            IEnumerable<NodePart> parts = creator.GetNodeParts();
            Assert.False(parts.Any());

            Directory.Delete(dirName);
        }

        [Fact]
        public async Task Create()
        {
            PartsCreator creator = new(
                TestData.DirToCompress, new ModesPart());

            bool success = await creator.Prepare();
            Assert.True(success);

            IEnumerable<NodePart> parts = creator.GetNodeParts();

            new NodeTree(parts, (ulong)creator.FilesData.LongCount())
                .Lookup((entryName, isDir, inx) =>
                {
                    string dirToCompressEntryName =
                        $"{TestData.DirToCompress}{entryName}";

                    if (isDir)
                    {
                        Assert.True(Directory.Exists(
                            dirToCompressEntryName));
                    }
                    else
                    {
                        Assert.True(File.Exists(
                            dirToCompressEntryName));
                    }
                });
        }
    }
}