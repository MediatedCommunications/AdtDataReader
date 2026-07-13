namespace AdtDataReader.Storage;

public record AdtDataFileColumn  {
    public string? Name { get; init; }
    public AdtDataType DataType { get; init; }
    public int Offset { get; init; }
    public int Length { get; init; }
    public byte Scale { get; init; }

}
