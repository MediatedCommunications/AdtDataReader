using Microsoft.Win32.SafeHandles;
using System.Collections.Immutable;
using System.Text;

namespace AdtDataReader.Storage;

public class AdtDataFile : IDisposable {
    private bool disposedValue;

    protected SafeFileHandle Reader { get; }

    public Encoding Encoding { get; }
    public AdtDataFileHeader Header { get; }
    public ImmutableArray<AdtDataFileColumn> Columns { get; }

    internal AdtDataFile(SafeFileHandle Reader, Encoding Encoding, AdtDataFileHeader Header, ImmutableArray<AdtDataFileColumn> Columns) {
        this.Reader = Reader;
        this.Encoding = Encoding;
        this.Header = Header;
        this.Columns = Columns;
    }

    public byte[]? ReadBytes(uint Row) {
        var ret = new byte[Header.RowSize];

        RandomAccess.Read(Reader, ret, Header.RowOffset + (Header.RowSize * Row));

        return ret;
    }

    public IEnumerable<AdtDataFileRow> ReadRows() {
        for (uint RowNumber = 0; RowNumber < Header.RowCount; RowNumber++) {
            if (ReadRow(RowNumber) is { } Row) {
                yield return Row;
            }
        }
    }

    public AdtDataFileRow? ReadRow(uint Row) {
        var ret = default(AdtDataFileRow?);

        if(ReadBytes(Row) is { } Bytes && Bytes.Length > 0) {

            var IsDeleted = ((Bytes[0] & 1) == 1);

            var Values = new List<object?>(Columns.Length);

            foreach(var Column in Columns) {

                var CellBytes = Bytes[Column.Offset..][..Column.Length];

                var Value = AdtDataTypeParser.Parse(Column.DataType, CellBytes, Encoding);
                Values.Add(Value);
            }
            ret = new() {
                Row = Row,
                IsDeleted = IsDeleted,
                Values = [.. Values],
            };
        }
        

        return ret;
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                Reader.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
