using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataCompressor.Parts
{
    public class PartsCreator
    {
        private readonly List<(string, ulong)> FilesAndIndices;
        private readonly List<(string, ulong)> DirsAndIndices;

        private readonly List<NodePart> FileNodes;
        private readonly List<NodePart> DirNodes;

        private readonly HashSet<ulong> RootFilesIndices;
        private bool Prepared;
        private readonly string DirName;

        /// <summary>
        /// Count of created nodes
        /// </summary>
        public long NodesCount { get; private set; }

        private ModesPart Modes;

        /// <summary>
        /// Enumerable of files data
        /// </summary>
        public List<byte[]> FilesData { get; private set; }

        public PartsCreator(string dirName, ModesPart modes)
        {
            DirName = dirName;

            FileNodes = new();
            DirNodes = new();
            FilesAndIndices = new();
            DirsAndIndices = new();

            FilesData = new();
            RootFilesIndices = new();

            Prepared = false;
            Modes = modes;
        }

        private async Task LookupRec(string dirName,
            ulong? parentIndex)
        {
            foreach (string entry in Directory
                .EnumerateFileSystemEntries(dirName))
            {
                if (File.Exists(entry))
                {
                    string fileName = new FileInfo(entry).Name;
                    if (parentIndex.HasValue)
                    {
                        FilesAndIndices.Add(
                            (fileName, parentIndex.Value));
                    }
                    else
                    {
                        ulong fileIndex = (ulong)FilesAndIndices.LongCount();
                        FilesAndIndices.Add((fileName, fileIndex));
                        RootFilesIndices.Add(fileIndex);
                    }

                    byte[] data = await File.ReadAllBytesAsync(entry);
                    FilesData.Add(data);
                    continue;
                }

                ulong dirsCount = (ulong)DirsAndIndices.LongCount();

                DirsAndIndices.Add((
                    new DirectoryInfo(entry).Name,
                    parentIndex.HasValue ?
                        parentIndex.Value : dirsCount));

                await LookupRec(entry, dirsCount);
            }
        }

        private bool RepairParentIndices()
        {
            ulong filesCount = (ulong)FileNodes.LongCount();

            IEnumerator<NodePart> filesEnumerator = FileNodes.GetEnumerator();

            ulong i = 0;

            while (filesEnumerator.MoveNext())
            {
                if (!RootFilesIndices.Contains(i)
                    && !filesEnumerator.Current.ParentIndex.Add(filesCount))
                {
                    return false;
                }

                i++;
            }

            IEnumerator<NodePart> dirsEnumerator = DirNodes.GetEnumerator();
            while (dirsEnumerator.MoveNext())
            {
                if (!dirsEnumerator.Current.ParentIndex.Add(filesCount))
                {
                    return false;
                }
            }

            return true;
        }

        private bool PrepareSetNodes()
        {
            foreach ((string fileName, ulong parentIndex) in FilesAndIndices)
            {
                NodePart node = new(fileName, Modes);
                if (!node.SetParentIndex(parentIndex))
                {
                    FileNodes.Clear();
                    return false;
                }

                FileNodes.Add(node);
            }

            foreach ((string dirName, ulong parentIndex) in DirsAndIndices)
            {
                NodePart node = new(dirName, Modes);
                if (!node.SetParentIndex(parentIndex))
                {
                    FileNodes.Clear();
                    DirNodes.Clear();
                    return false;
                }

                DirNodes.Add(node);
            }

            if (RepairParentIndices())
            {
                return true;
            }

            FileNodes.Clear();
            DirNodes.Clear();
            return false;
        }

        /// <summary>
        /// Prepares nodes and data. This method should be called 
        /// immediately after initialization. It may modify Modes.
        /// </summary>
        /// <returns>Successfully prepared</returns>
        public async Task<bool> Prepare()
        {
            if (Prepared)
            {
                return true;
            }

            if (Directory
                .EnumerateDirectories(DirName).Any())
            {
                Modes.SetOn(Mode.FileTreeMode);
            }

            await LookupRec(DirName, null);

            if (!Modes.ChooseIntegerModes(
                Mode.NodesShortMode, Mode.NodesIntMode,
                () => PrepareSetNodes()
            ))
            {
                return false;
            }

            NodesCount = FileNodes.LongCount() + DirNodes.LongCount();
            Prepared = true;
            return true;
        }

        public IEnumerable<NodePart> GetNodeParts()
        {
            foreach (NodePart file in FileNodes)
            {
                yield return file;
            }

            foreach (NodePart dir in DirNodes)
            {
                yield return dir;
            }
        }
    }
}
