using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DataCompressor.Test
{
    public class CreatorTest
    {
        [Fact]
        public async Task CreateEmpty_ShouldSucceed()
        {
            string outputFileName =
                $"{TestData.DataDir}/CreatorEmpty{Settings.OutputFileExtension}";

            string emptyDirName =
                $"{TestData.DataDir}/CreatorEmpty";
            Directory.CreateDirectory(emptyDirName);

            Creator creator = new(emptyDirName, outputFileName);
            await creator.Compress();

            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
            Directory.Delete(emptyDirName);
        }

        [Fact]
        public async Task CreateWholeToSameDir_ShouldSucceed()
        {
            Creator creator = new(TestData.DirToCompress);
            await creator.Compress();

            string outputFileName =
                $"{TestData.DirToCompress}{Settings.OutputFileExtension}";
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Fact]
        public async Task CreateWhole_ShouldSucceed()
        {
            string outputFileName =
                $"{TestData.DirToCompress}{Settings.OutputFileExtension}";
            Creator creator = new(TestData.DirToCompress, outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }
    }
}