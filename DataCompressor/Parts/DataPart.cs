using DataCompressor.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DataCompressor.Parts
{
    public class DataPart
    {
        /// <summary>
        /// Files parts it consists of
        /// </summary>
        public IEnumerable<FilePart> FilesParts { get; private set; }

        /// <summary>
        /// Count of files parts
        /// </summary>
        public DynamicInteger Count { get; private set; }

        /// <summary>
        /// Reconstructs data part
        /// </summary>
        /// <param name="bytesEnumerator">Bytes enumerator which is 
        /// moved only by such a count of bytes so it can be 
        /// reconstructed.</param>
        /// <param name="modes">Modes used</param>
        public DataPart(
            ConstructEnumerator bytesEnumerator, ModesPart modes)
        {
            int bytesCount = ModesPart.GetBytesCount(
                modes.IsOn(Mode.NodesShortMode),
                modes.IsOn(Mode.NodesIntMode));

            Count = new DynamicInteger(bytesCount,
                bytesEnumerator.GetMore(bytesCount));

            FilePart[] filesParts = new FilePart[Count.Value];

            for (ulong i = 0; i < Count.Value; i++)
            {
                filesParts[i] = new(bytesEnumerator, modes);
            }

            FilesParts = filesParts;
        }

        /// <summary>
        /// Creates data part.
        /// </summary>
        /// <param name="filesBytes">Files bytes to create from</param>
        /// <param name="count">Count of files bytes</param>
        /// <param name="modes">Modes used</param>
        public DataPart(
            IEnumerable<byte[]> filesBytes, long count,
            ModesPart modes)
        {
            FilesParts = filesBytes.Select(data =>
                new FilePart(data, modes)).ToList();

            int bytesCount = ModesPart.GetBytesCount(
                modes.IsOn(Mode.NodesShortMode),
                modes.IsOn(Mode.NodesIntMode));

            Count = new DynamicInteger(bytesCount);
            Count.Set((ulong)count);
        }

        public bool PrepareBytes()
        {
            return FilesParts.All(data => data.PrepareBytes());
        }

        public IEnumerable<byte> GetBytes()
        {
            foreach (byte b in Count.Bytes)
            {
                yield return b;
            }

            foreach (FilePart compressedFile in FilesParts)
            {
                foreach (byte b in compressedFile.GetBytes())
                {
                    yield return b;
                }
            }
        }
    }
}
