namespace AdtDataReader.Storage;

public record AdtMemoFileReference {
    public int BlockNumber { get; init; }
    public int Length { get; init; }
}
