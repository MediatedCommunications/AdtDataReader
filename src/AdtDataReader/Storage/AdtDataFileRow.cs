using System.Collections.Immutable;

namespace AdtDataReader.Storage;

public record AdtDataFileRow {
    public uint Row { get; init; }
    public bool IsDeleted { get; init; }
    public ImmutableArray<object?> Values { get; init; } = [];
}
