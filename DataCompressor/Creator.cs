using DataCompressor.Exceptions;
using DataCompressor.Parts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataCompressor
{
    public class Creator
    {
        private readonly string DirName;
        private readonly string OutputFileName;

        private bool Prepared;

        private ModesPart Modes;
        private IEnumerable<byte> NodesBytes;
        private byte[] DataBytes;

        /// <param name="dirName">Root directory including path</param>
        /// <param name="outputFileName">Output file including path. File named
        /// after directory placed into the parent directory will be used if not
        /// specified.</param>
        /// <exception cref="UnableToConstructException">
        /// Thrown when provided directory has no parent and output 
        /// file was not specified.</exception>
        public Creator(string dirName, string? outputFileName = null)
        {
            DirName = dirName;
            DirectoryInfo dirInfo = new(DirName);

            if (string.IsNullOrEmpty(outputFileName))
            {
                if (dirInfo.Parent == null)
                {
                    throw new UnableToConstructException();
                }

                string parentDirName = dirInfo.Parent.FullName;
                OutputFileName = Path.Combine(parentDirName, dirInfo.Name +
                    Settings.OutputFileExtension);
            }
            else
            {
                OutputFileName = outputFileName;
            }

            DataBytes = Array.Empty<byte>();
            NodesBytes = Array.Empty<byte>();
            Modes = new();
        }

        private bool PrepareSetDataBytes(List<byte[]> filesData)
        {
            return Modes.ChooseIntegerModes(
                Mode.RepeatCountShortMode,
                Mode.RepeatCountIntMode,
                () => Modes.ChooseIntegerModes(
                        Mode.FragmentsCountShortMode,
                        Mode.FragmentsCountIntMode,
                    () =>
                    {
                        DataPart part = new(filesData, filesData.LongCount(),
                            Modes);

                        if (part.PrepareBytes())
                        {
                            DataBytes = part.GetBytes().ToArray();
                            return true;
                        }

                        return false;
                    })
                );
        }

        private async Task<bool> Prepare()
        {
            if (Prepared)
            {
                return true;
            }

            PartsCreator creator = new(DirName, Modes);

            bool success = await creator.Prepare();
            if (!success)
            {
                return false;
            }

            Modes.SetOn(Mode.RepeatMode);
            bool preparedDataBytesWithRepeat =
                PrepareSetDataBytes(creator.FilesData);

            ModesPart modesWithRepeat = Modes;
            byte[] dataBytesWithRepeat = DataBytes;
            Modes = new ModesPart(Modes.ModesByte)
                .SetOff(Mode.RepeatMode);

            bool preparedDataBytes = PrepareSetDataBytes(creator.FilesData);

            if (!preparedDataBytes && !preparedDataBytesWithRepeat)
            {
                return false;
            }

            if (!preparedDataBytes ||
                dataBytesWithRepeat.Length < DataBytes.Length)
            {
                Modes = modesWithRepeat;
                DataBytes = dataBytesWithRepeat;
            }

            NodesBytes = new NodesPart(creator.GetNodeParts(),
                creator.NodesCount,
                Modes).GetBytes();

            Prepared = true;
            return true;
        }

        private IEnumerable<byte> GetOutputBytes()
        {
            yield return Modes.ModesByte;

            foreach (byte b in NodesBytes)
            {
                yield return b;
            }

            foreach (byte b in DataBytes)
            {
                yield return b;
            }
        }

        public async Task Compress()
        {
            bool success = await Prepare();
            if (success)
            {
                byte[] outputBytes = GetOutputBytes().ToArray();
                await File.WriteAllBytesAsync(OutputFileName, outputBytes);
            }
            else
            {
                throw new UnableToConstructException();
            }
        }
    }
}
