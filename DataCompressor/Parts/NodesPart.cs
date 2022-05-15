using DataCompressor.Helpers;
using System.Collections.Generic;

namespace DataCompressor.Parts
{
    public class NodesPart
    {
        /// <summary>
        /// Created nodes parts
        /// </summary>
        public IEnumerable<NodePart> NodesParts { get; private set; }

        /// <summary>
        /// Count of nodes
        /// </summary>
        public DynamicInteger Count { get; private set; }

        /// <summary>
        /// Reconstructs nodes part.
        /// </summary>
        /// <param name="bytesEnumerator">Bytes enumerator which is moved  
        /// only by such a count of bytes so it can be reconstructed.</param>
        /// <param name="modes">Used modes</param>
        public NodesPart(ConstructEnumerator bytesEnumerator, ModesPart modes)
        {
            int bytesCount = ModesPart.GetBytesCount(
                    modes.IsOn(Mode.NodesShortMode),
                    modes.IsOn(Mode.NodesIntMode));

            Count = new DynamicInteger(bytesCount,
                bytesEnumerator.GetMore(bytesCount));

            NodePart[] nodes = new NodePart[Count.Value];

            for (ulong i = 0; i < Count.Value; i++)
            {
                nodes[i] = new(bytesEnumerator, modes);

                if (modes.IsOff(Mode.FileTreeMode))
                {
                    nodes[i].SetParentIndex(i);
                }
            }

            NodesParts = nodes;
        }


        /// <summary>
        /// Creates nodes part.
        /// </summary>
        /// <param name="nodes">Nodes to create from</param>
        /// <param name="nodesCount">Count of nodes</param>
        /// <param name="modes">used modes</param>
        public NodesPart(IEnumerable<NodePart> nodes,
            long nodesCount, ModesPart modes)
        {
            NodesParts = nodes;

            int bytesCount = ModesPart.GetBytesCount(
                modes.IsOn(Mode.NodesShortMode),
                modes.IsOn(Mode.NodesIntMode));

            Count = new DynamicInteger(bytesCount);
            Count.Set((ulong)nodesCount);
        }

        public IEnumerable<byte> GetBytes()
        {
            foreach (byte b in Count.Bytes)
            {
                yield return b;
            }

            foreach (NodePart node in NodesParts)
            {
                foreach (byte b in node.GetBytes())
                {
                    yield return b;
                }
            }
        }
    }
}
