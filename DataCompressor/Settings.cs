using System.Collections.Generic;

namespace DataCompressor
{
    /// <summary>
    /// Unit tests expect default settings.
    /// </summary>
    public class Settings
    {
        public static readonly string OutputFileExtension = ".dc";

        public static readonly Dictionary<Mode, int> ModeAndBit = new()
        {
            { Mode.RepeatMode, 0 },
            { Mode.FileTreeMode, 1 },
            { Mode.NodesShortMode, 2 },
            { Mode.NodesIntMode, 3 },
            { Mode.FragmentsCountShortMode, 4 },
            { Mode.FragmentsCountIntMode, 5 },
            { Mode.RepeatCountShortMode, 6 },
            { Mode.RepeatCountIntMode, 7 },
        };

        public static readonly HashSet<Mode> DefaultModesOn = new()
        {
            Mode.RepeatMode,
            Mode.RepeatCountIntMode,
            Mode.FragmentsCountIntMode,
            Mode.NodesIntMode,
        };
    }
}
