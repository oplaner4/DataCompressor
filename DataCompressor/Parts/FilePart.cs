using DataCompressor.Helpers;
using System;
using System.Collections.Generic;

namespace DataCompressor.Parts
{
    public class FilePart
    {
        private readonly ModesPart Modes;

        /// <summary>
        /// Fragments it consists of
        /// </summary>
        public readonly IEnumerable<byte> Fragments;

        private IEnumerable<byte> BytesWithoutCount;
        private DynamicInteger Count;

        /// <summary>
        /// Creates file part.
        /// </summary>
        /// <param name="data">File data to create from</param>
        /// <param name="modes">Modes used</param>
        public FilePart(IEnumerable<byte> data, ModesPart modes)
        {
            Modes = modes;
            Fragments = data;
            BytesWithoutCount = Array.Empty<byte>();
            Count = new DynamicInteger(0);
        }

        /// <summary>
        /// Reconstructs file part.
        /// </summary>
        /// <param name="bytesEnumerator">Bytes enumerator which is moved  
        /// only by such a count of bytes so it can be reconstructed.</param>
        /// <param name="modes">used modes</param>
        public FilePart(
            ConstructEnumerator bytesEnumerator, ModesPart modes)
        {
            Modes = modes;
            BytesWithoutCount = Array.Empty<byte>();

            List<byte> fragments = new();

            int fragmentsBytesCount = ModesPart.GetBytesCount(
                modes.IsOn(Mode.FragmentsCountShortMode),
                modes.IsOn(Mode.FragmentsCountIntMode));

            int repeatBytesCount = ModesPart.GetBytesCount(
                modes.IsOn(Mode.RepeatCountShortMode),
                modes.IsOn(Mode.RepeatCountIntMode));

            Count = new DynamicInteger(fragmentsBytesCount,
                bytesEnumerator.GetMore(fragmentsBytesCount));

            for (ulong i = 0; i < Count.Value; i++)
            {
                bool notLast = i < Count.Value - 1;
                byte b = bytesEnumerator.GetCurrent();

                if (Modes.IsOn(Mode.RepeatMode))
                {
                    bytesEnumerator.MoveNext(notLast, "Lack of bytes for fragments");
                    ulong repeat = new DynamicInteger(repeatBytesCount,
                        bytesEnumerator.GetMore(repeatBytesCount)).Value;

                    while (repeat > 0)
                    {
                        fragments.Add(b);
                        repeat--;
                    }
                }
                else
                {
                    fragments.Add(b);
                    bytesEnumerator.MoveNext(notLast, "Lack of bytes for fragments");
                }
            }

            Fragments = fragments;
        }

        public bool PrepareBytes()
        {
            ulong count = 0;
            int bytesCount = ModesPart.GetBytesCount(
                Modes.IsOn(Mode.FragmentsCountShortMode),
                Modes.IsOn(Mode.FragmentsCountIntMode));

            Count = new DynamicInteger(bytesCount);
            ulong maxCount = Count.GetMaxValue();

            int repeatBytesCount = ModesPart.GetBytesCount(
                Modes.IsOn(Mode.RepeatCountShortMode),
                Modes.IsOn(Mode.RepeatCountIntMode));

            List<byte> bytesWithoutCount = new();
            IEnumerator<byte> enumerator = Fragments.GetEnumerator();
            bool anotherBytes = enumerator.MoveNext();

            while (anotherBytes)
            {
                byte fragment = enumerator.Current;
                bytesWithoutCount.Add(fragment);
                if (++count > maxCount)
                {
                    return false;
                }

                anotherBytes = enumerator.MoveNext();

                if (Modes.IsOn(Mode.RepeatMode))
                {
                    DynamicInteger repeat = new(repeatBytesCount);
                    ulong repeatValue = 1;
                    ulong maxRepeatValue = repeat.GetMaxValue();

                    while (anotherBytes && enumerator.Current == fragment)
                    {
                        if (++repeatValue > maxRepeatValue)
                        {
                            return false;
                        }
                        anotherBytes = enumerator.MoveNext();
                    }

                    repeat.Set(repeatValue);

                    foreach (byte b in repeat.Bytes)
                    {
                        bytesWithoutCount.Add(b);
                    }
                }
            }

            BytesWithoutCount = bytesWithoutCount;
            Count.Set(count);
            return true;
        }


        public IEnumerable<byte> GetBytes()
        {
            foreach (byte b in Count.Bytes)
            {
                yield return b;
            }

            foreach (byte b in BytesWithoutCount)
            {
                yield return b;
            }
        }
    }
}
