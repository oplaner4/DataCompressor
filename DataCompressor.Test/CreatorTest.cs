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
                $"{TestData.DataDir}/CreateEmpty{Settings.OutputFileExtension}";
            Creator creator = new(
                $"{TestData.DirToCompress}/Empty", outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Fact]
        public async Task CreateWholeToSameDir_ShouldSucceed()
        {
            string outputFileName =
                $"{TestData.DataDir}/DirToCompress{Settings.OutputFileExtension}";
            Creator creator = new(TestData.DirToCompress);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Fact]
        public async Task CreateWhole_ShouldSucceed()
        {
            string outputFileName =
                $"{TestData.DataDir}/DirToCompress{Settings.OutputFileExtension}";
            Creator creator = new(TestData.DirToCompress, outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Fact]
        public async Task CreateManyRepeatingChars_ShouldSucceed()
        {
            string outputFileName =
                $"{TestData.DataDir}/ManyRepeatingChars{Settings.OutputFileExtension}";
            Creator creator = new($"{TestData.DataDir}/ManyRepeatingChars", outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }
    }
}