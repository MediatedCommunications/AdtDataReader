namespace AdtDataReader.Storage;

public record AdtMemoFileHeader {
    public uint NextFreeBlock { get; init; }
    public ushort BlockSize { get; init; }
}
