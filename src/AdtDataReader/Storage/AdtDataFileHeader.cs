namespace AdtDataReader.Storage;

public record AdtDataFileHeader {
    public uint RowCount { get; init; }
    public uint RowOffset { get; init; }
    public ushort RowSize { get; init; }
    public uint ColumnCount { get; init; }
    public string? DictionaryFileName { get; init; }
}
