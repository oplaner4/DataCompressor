using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using DataCompressor.Parts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataCompressor
{
    public class Reconstructor
    {
        private readonly string CompressedName;
        private readonly string OutputDirName;

        /// <param name="compressedName">Previously compressed file including 
        /// path</param>
        /// <param name="outputDirName">Output directory including path. Directory
        /// a compressed file is in will be used if not specified.</param>
        /// <exception cref="UnableToConstructException">Thrown when file is not in
        /// any directory.</exception>
        public Reconstructor(string compressedName,
            string? outputDirName = null)
        {
            CompressedName = compressedName;

            if (outputDirName == null)
            {
                outputDirName = new FileInfo(CompressedName).DirectoryName;
            }

            if (outputDirName == null)
            {
                throw new UnableToConstructException();
            }

            OutputDirName = outputDirName;
        }

        public async Task Decompress()
        {
            byte[] OutputBytes = await File.ReadAllBytesAsync(CompressedName);

            ConstructEnumerator enumerator = new(OutputBytes);

            ModesPart modesPart = new(enumerator.GetCurrent());
            enumerator.MoveNext();

            NodesPart nodesPart = new(enumerator, modesPart);
            DataPart dataPart = new(enumerator, modesPart);

            NodeTree tree = new(nodesPart.NodesParts, dataPart.Count.Value);

            FilePart[] fileParts = dataPart.FilesParts.ToArray();

            tree.Lookup(async (fullName, isDir, inx) =>
            {
                if (isDir)
                {
                    Directory.CreateDirectory(OutputDirName + fullName);
                }
                else
                {
                    await File.WriteAllBytesAsync(
                        OutputDirName + fullName,
                        fileParts[inx].Fragments.ToArray());
                }
            });
        }
    }
}
