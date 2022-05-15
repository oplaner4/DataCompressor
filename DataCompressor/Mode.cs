namespace DataCompressor
{
    public enum Mode
    {
        // Repeat bytes are present after data fragment.
        RepeatMode,

        // Parent index bytes are present before node name.
        FileTreeMode,

        // Combination of these modes determines 
        // integer type of nodes count and
        // node parent index.
        NodesShortMode,
        NodesIntMode,

        // Combination of these modes determines
        // integer type of fragments count.
        FragmentsCountShortMode,
        FragmentsCountIntMode,

        // Combination of these modes determines
        // integer type of repeat count in repeat mode.
        RepeatCountShortMode,
        RepeatCountIntMode,
    }
}
