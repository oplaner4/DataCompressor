using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DataCompressor.Test
{
    public class ReconstructorTest
    {
        [Fact]
        public async Task ReconstructIntoSameDir_ShouldSucceed()
        {
            string subdirectoryName =
                $"{TestData.DataDir}/Reconstructed_DirToCompress";
            Directory.CreateDirectory(subdirectoryName);

            string outputFileName =
                $"{subdirectoryName}/DirToCompress{Settings.OutputFileExtension}";

            Creator creator = new(TestData.DirToCompress, outputFileName);
            await creator.Compress();
            Assert.True(File.Exists(outputFileName));


            Reconstructor reconstructor = new(outputFileName);
            await reconstructor.Decompress();

            await VerifySameDirectories(TestData.DirToCompress, subdirectoryName);

            Directory.Delete(subdirectoryName, true);
        }

        private async Task VerifySameDirectories(
            string firstDirName, string secondDirName)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(
                    firstDirName, "*",
                    new EnumerationOptions() { RecurseSubdirectories = true }
                ))
            {
                string relEntry = Path.GetRelativePath(firstDirName, entry);
                string outputEntry = $"{secondDirName}/{relEntry}";

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
        }

        [Fact]
        public async Task Reconstruct_ShouldSucceed()
        {
            foreach (string dirName in
                Directory.EnumerateDirectories(TestData.DirToCompress, "*",
                    new EnumerationOptions() { RecurseSubdirectories = true }
                )
            )
            {
                string relDirName = Path.GetRelativePath(TestData.DataDir, dirName);
                string outputFileName =
                    $"{TestData.DataDir}/{relDirName.Replace('\\', '_')}" +
                    $"{Settings.OutputFileExtension}";

                Creator creator = new(dirName, outputFileName);
                await creator.Compress();
                Assert.True(File.Exists(outputFileName));

                string outputDirName =
                    $"{TestData.DataDir}/Reconstructed_{relDirName.Replace('\\', '_')}";
                Directory.CreateDirectory(outputDirName);

                Reconstructor reconstructor =
                    new(outputFileName, outputDirName);
                await reconstructor.Decompress();

                await VerifySameDirectories(dirName, outputDirName);

                Directory.Delete(outputDirName, true);
                File.Delete(outputFileName);
            }
        }
    }
}