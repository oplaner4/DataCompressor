using DataCompressor.Parts;
using System.Collections.Generic;
using System.Linq;

namespace DataCompressor.Helpers
{
    public class NodeTree
    {
        private readonly NodePart[] Nodes;
        private readonly ulong FilesCount;

        public delegate void LookupAction(
            string fullName, bool isDir, ulong nodeIndex);

        /// <param name="nodesParts">Nodes to create from.
        /// File nodes must be listed at first.</param>
        /// <param name="filesCount">Count of nodes parts where node is 
        /// file.</param>
        public NodeTree(IEnumerable<NodePart> nodesParts,
            ulong filesCount)
        {
            Nodes = nodesParts.ToArray();
            FilesCount = filesCount;
        }

        /// <summary>
        /// Goes through nodes like in file system.
        /// </summary>
        /// <param name="action">Action to be done with entry</param>
        public NodeTree Lookup(LookupAction action)
        {
            LookupRec(action, string.Empty, null);
            return this;
        }

        private void LookupRec(
            LookupAction action,
            string path, ulong? parentIndex)
        {
            ulong nodeIndex = 0;

            foreach (NodePart nodePart in Nodes)
            {
                bool isRootChild = parentIndex == null &&
                    nodeIndex == nodePart.ParentIndex.Value;

                bool isChild =
                    nodeIndex != nodePart.ParentIndex.Value
                    && parentIndex == nodePart.ParentIndex.Value;

                if (isRootChild || isChild)
                {
                    string fullName = $"{path}/{nodePart.Name}";
                    bool isDirectory = nodeIndex >= FilesCount;
                    action(fullName, isDirectory, nodeIndex);

                    if (isDirectory)
                    {
                        LookupRec(action, fullName, nodeIndex);
                    }
                }

                nodeIndex++;
            }
        }
    }
}
