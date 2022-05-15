using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DataCompressor.Test
{
    public class ReconstructorTest
    {
        [Fact]
        public async Task ReconstructWholeIntoSameDir_ShouldSucceed()
        {
            string subdirectoryName =
                $"{TestData.DataDir}/ReconstructedWhole";
            Directory.CreateDirectory(subdirectoryName);

            string outputFileName =
                $"{subdirectoryName}/DirToCompress{Settings.OutputFileExtension}";

            Creator creator = new(TestData.DirToCompress, outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));


            Reconstructor reconstructor = new(outputFileName);
            await reconstructor.Decompress();

            foreach (string entry in Directory.EnumerateFileSystemEntries(
                TestData.DirToCompress,
                "*",
                new EnumerationOptions() { RecurseSubdirectories = true }
            ))
            {
                string relEntry = Path.GetRelativePath(
                    TestData.DirToCompress, entry);
                string outputEntry = $"{subdirectoryName}/{relEntry}";
                Assert.True(File.Exists(outputEntry) ||
                    Directory.Exists(outputEntry));
            }

            Directory.Delete(subdirectoryName, true);
        }

        [Theory]
        [InlineData("DirToCompress/Empty")]
        [InlineData("DirToCompress/Media")]
        [InlineData("DirToCompress/Media/Interesting")]
        [InlineData("DirToCompress")]
        [InlineData("Basic")]
        [InlineData("ManyRepeatingChars")]
        public async Task Reconstruct_ShouldSucceed(
            string dirName)
        {
            string fullDirName = $"{TestData.DataDir}/{dirName}";
            string outputFileName =
                $"{TestData.DataDir}/{dirName.Replace('/', '_')}{Settings.OutputFileExtension}";
            Creator creator = new(fullDirName, outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));

            string outputDirName =
                $"{TestData.DataDir}/Reconstructed{dirName.Replace('/', '_')}";
            Directory.CreateDirectory(outputDirName);

            Reconstructor reconstructor =
                new(outputFileName, outputDirName);
            await reconstructor.Decompress();

            foreach (string entry in Directory.EnumerateFileSystemEntries(
                fullDirName, "*",
                new EnumerationOptions() { RecurseSubdirectories = true }
            ))
            {
                string relEntry = Path.GetRelativePath(
                    fullDirName, entry);
                string outputEntry = $"{outputDirName}/{relEntry}";

                if (File.Exists(outputEntry))
                {
                    byte[] entryData = await File.ReadAllBytesAsync(entry);
                    byte[] outputEntryData =
                        await File.ReadAllBytesAsync(outputEntry);

                    Assert.Equal(entryData, outputEntryData);
                    continue;
                }

                Assert.True(Directory.Exists(outputEntry));
            }

            Directory.Delete(outputDirName, true);
            File.Delete(outputFileName);
        }
    }
}