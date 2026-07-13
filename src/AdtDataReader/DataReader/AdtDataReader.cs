using AdtDataReader.Storage;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace AdtDataReader;


public class AdtDataReader : DbDataReader, IDbColumnSchemaGenerator {
    private readonly AdtDataReaderOptions Options;
    
    private AdtDataFile DataFile { get; set; }
    private AdtMemoFile? MemoFile { get; set; }
    
    private uint NextRow { get; set; }
    private AdtDataFileRow? CurrentRowData { get; set; }

    public AdtDataReader(string DataFilePath, string? MemoFilePath)
        : this(DataFilePath, MemoFilePath, new AdtDataReaderOptions()) {
    }

    public AdtDataReader(string DataFilePath, string? MemoFilePath, AdtDataReaderOptions Options) {
        this.DataFile = AdtDataFiles.OpenRead(DataFilePath);
        this.MemoFile = default(AdtMemoFile?);
       
        this.Options = Options; 
    }

    public override void Close() {
        try {
            DataFile?.Dispose();
            MemoFile?.Dispose();
        } finally {
            DataFile = null;
            MemoFile = null;
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            Close();
        }
    }



    public override bool GetBoolean(int ordinal) {
        return (bool)GetValue(ordinal);
    }

    public override byte GetByte(int ordinal) {
        return (byte)GetValue(ordinal);
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal) {
        return (char)GetValue(ordinal);
    }

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal) {
        var AdtColumn = DataFile.Columns[ordinal];
        return AdtColumn.DataType.ToString();
    }

    public override DateTime GetDateTime(int ordinal) {
        return (DateTime) GetValue(ordinal);
    }

    public override decimal GetDecimal(int ordinal) {
        return (decimal)GetValue(ordinal);
    }

    public override double GetDouble(int ordinal) {
        return (double) GetValue(ordinal);
    }

    public override IEnumerator GetEnumerator() {
        return new DbEnumerator(this, closeReader: false);
    }

    public override bool NextResult() {
        return false;
    }

    public override bool Read() {
        var ret = false;
        var NewCurrentRowData = default(AdtDataFileRow?);

        while(NextRow < DataFile.Header.RowCount) {
            var Row = NextRow;
            this.NextRow += 1;

            if(DataFile.ReadRow(Row) is { } RowData) {
                
                if(Options.SkipDeletedRecords && RowData.IsDeleted == true) {
                    // Do Nothing
                } else {
                    NewCurrentRowData = RowData;
                    break;
                }

            } else {
                break;
            }

        }

        if(NewCurrentRowData is { }) {
            ret = true;
            this.CurrentRowData = NewCurrentRowData;
        }

        return ret;

    }

    public override int Depth => throw new NotImplementedException();

    public override bool IsClosed => DataFile is null;

    public override int RecordsAffected => throw new NotImplementedException();

    public override object this[string name] {
        get {
            var ordinal = GetOrdinal(name);
            return GetValue(ordinal);
        }
    }

    public override object this[int ordinal] => GetValue(ordinal);

    public override int FieldCount => DataFile.Columns.Length;

    public override bool HasRows => DataFile.Header.RowCount > 0;

    public override bool IsDBNull(int ordinal) {
        var value = GetValue(ordinal);
        return value == null;
    }

    public override int GetValues(object[] values) {
        for (var ordinal = 0; ordinal < FieldCount; ordinal++) {
            values[ordinal] = GetValue(ordinal);
        }
        return FieldCount;
    }

    public override object GetValue(int ordinal) {

        var ret = CurrentRowData?.Values[ordinal];

        if(ret is AdtMemoFileReference { } Ref && MemoFile is { }) {
            ret = MemoFile.ReadValue(Ref, DataFile.Columns[ordinal], DataFile.Encoding);
        }

        return ret;
    }

    public override string GetString(int ordinal) {
        return (string) GetValue(ordinal);
    }

    public override int GetOrdinal(string name) {
        var ordinal = 0;

        foreach (var AdtColumn in DataFile.Columns) {
            if (AdtColumn.Name == name) return ordinal;
            ordinal++;
        }
        ordinal = 0;
        foreach (var AdtColumn in DataFile.Columns) {
            if (String.Equals(AdtColumn.Name, name, StringComparison.OrdinalIgnoreCase)) return ordinal;
            ordinal++;
        }

        throw new IndexOutOfRangeException();
    }

    public override string GetName(int ordinal) {
        var AdtColumn = DataFile.Columns[ordinal];
        return AdtColumn.Name;
    }

    public override long GetInt64(int ordinal) {
        return (long) GetValue(ordinal);
    }

    public override int GetInt32(int ordinal) {
        return (int) GetValue(ordinal);
    }

    public override short GetInt16(int ordinal) {
        return (short) GetValue(ordinal);
    }

    public override Guid GetGuid(int ordinal) {
        throw new NotImplementedException();
    }

    public override float GetFloat(int ordinal) {
        return (float)GetValue(ordinal);
    }

    public override Type GetFieldType(int ordinal) {
        var ret = MemoFile is { }
            ? DataFile.Columns[ordinal].DataType.GetCrlTypeWithMemo()
            : DataFile.Columns[ordinal].DataType.GetCrlTypeWithoutMemo()
            ;
        return ret;
    }

    public ReadOnlyCollection<DbColumn> GetColumnSchema() {
        throw new NotImplementedException();
    }

    public override DataTable GetSchemaTable() {
        var columnSchema = GetColumnSchema();
        return GetSchemaTable(columnSchema);
    }

    private static DataTable GetSchemaTable(ReadOnlyCollection<DbColumn> columnSchema) {
        var table = new DataTable("SchemaTable") {
            Columns =
            {
                    new DataColumn(SchemaTableColumn.ColumnName, typeof(string)),
                    new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int)),
                    new DataColumn(SchemaTableColumn.ColumnSize, typeof(int)),
                    new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short)),
                    new DataColumn(SchemaTableColumn.NumericScale, typeof(short)),
                    new DataColumn(SchemaTableColumn.DataType, typeof(Type)),
                    new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool)),

                    new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string)),
                    new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string)),
                    new DataColumn(SchemaTableColumn.BaseTableName, typeof(string)),

                    new DataColumn(SchemaTableColumn.IsAliased, typeof(bool)),
                    new DataColumn(SchemaTableColumn.IsExpression, typeof(bool)),
                    new DataColumn(SchemaTableColumn.IsKey, typeof(bool)),
                    new DataColumn(SchemaTableColumn.IsLong, typeof(bool)),
                    new DataColumn(SchemaTableColumn.IsUnique, typeof(bool)),

                    new DataColumn(SchemaTableColumn.ProviderType, typeof(int)),
                    new DataColumn(SchemaTableColumn.NonVersionedProviderType, typeof(int)),
                }
        };

        object dbNull = DBNull.Value;
        foreach (var column in columnSchema) {
            var row = table.NewRow();
            row[0] = column.ColumnName;
            row[1] = column.ColumnOrdinal ?? dbNull;
            row[2] = column.ColumnSize ?? dbNull;
            row[3] = column.NumericPrecision ?? dbNull;
            row[4] = column.NumericScale ?? dbNull;
            row[5] = column.DataType ?? dbNull;
            row[6] = column.AllowDBNull ?? dbNull;

            row[7] = column.BaseColumnName ?? dbNull;
            row[8] = column.BaseSchemaName ?? dbNull;
            row[9] = column.BaseTableName ?? dbNull;

            row[10] = column.IsAliased ?? dbNull;
            row[11] = column.IsExpression ?? dbNull;
            row[12] = column.IsKey ?? dbNull;
            row[13] = column.IsLong ?? dbNull;
            row[14] = column.IsUnique ?? dbNull;

            var code = (int)Type.GetTypeCode(column.DataType);
            row[15] = code;
            row[16] = code;

            table.Rows.Add(row);
        }

        return table;
    }
}
