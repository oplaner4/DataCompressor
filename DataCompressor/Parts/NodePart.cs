using DataCompressor.Exceptions;
using DataCompressor.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataCompressor.Parts
{
    public class NodePart
    {
        private readonly ModesPart Modes;

        /// <summary>
        /// Index of the parent node. It is used in file tree mode.
        /// It is equal to index in NodePart[] if it has no parent.
        /// </summary>
        public DynamicInteger ParentIndex { get; internal set; }

        /// <summary>
        /// Name of the node without path
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Length of name (max 255 bits)
        /// </summary>
        public byte Len { get; private set; }

        /// <summary>
        /// Reconstructs node part.
        /// </summary>
        /// <param name="bytesEnumerator">Bytes enumerator which is moved  
        /// only by such a count of bytes so it can be 
        /// reconstructed.</param>
        /// <param name="modes">Modes used</param>
        /// <exception cref="UnableToConstructException">It is thrown 
        /// when there is lack of bytes or zero length of node name.</exception>
        public NodePart(ConstructEnumerator bytesEnumerator, ModesPart modes)
        {
            Modes = modes;
            int bytesCount = ModesPart.GetBytesCount(
                        modes.IsOn(Mode.NodesShortMode),
                        modes.IsOn(Mode.NodesIntMode));

            if (Modes.IsOn(Mode.FileTreeMode))
            {
                ParentIndex = new DynamicInteger(
                    bytesCount, bytesEnumerator.GetMore(bytesCount));
            }
            else
            {
                ParentIndex = new DynamicInteger(bytesCount);
            }

            Len = bytesEnumerator.GetCurrent();

            if (Len == 0)
            {
                throw new UnableToConstructException("Zero length of name");
            }

            bytesEnumerator.MoveNext(true, "Lack of bytes for node");

            byte[] nameBytes = bytesEnumerator.GetMore(Len);
            Name = Encoding.Default.GetString(nameBytes);
        }

        /// <summary>
        /// Creates node part.
        /// </summary>
        /// <param name="name">Name of the node. 
        /// It cannot be null or empty.</param>
        /// <param name="modes">Used modes</param>
        /// <param name="parentIndex">Index of the parent node. Zero implicitely.</param>
        public NodePart(string name, ModesPart modes, ulong? parentIndex = null)
        {
            Modes = modes;
            ParentIndex = new DynamicInteger(ModesPart.GetBytesCount(
                Modes.IsOn(Mode.NodesShortMode),
                Modes.IsOn(Mode.NodesIntMode)));

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(
                    nameof(name), "Null or empty name");
            }

            if (parentIndex.HasValue &&
                !SetParentIndex(parentIndex.Value))
            {
                throw new UnableToConstructException();
            }

            Name = name;
            Len = (byte)Name.Length;
        }

        public bool SetParentIndex(ulong parentIndex)
        {
            ParentIndex = new DynamicInteger(ModesPart.GetBytesCount(
                Modes.IsOn(Mode.NodesShortMode),
                Modes.IsOn(Mode.NodesIntMode)));

            return ParentIndex.Set(parentIndex);
        }

        public IEnumerable<byte> GetBytes()
        {
            if (Modes.IsOn(Mode.FileTreeMode))
            {
                foreach (byte b in ParentIndex.Bytes)
                {
                    yield return b;
                }
            }

            yield return Len;

            foreach (byte b in Encoding.UTF8.GetBytes(Name))
            {
                yield return b;
            }
        }
    }
}
