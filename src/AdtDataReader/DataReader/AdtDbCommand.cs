using System.Data;
using System.Data.Common;

namespace AdtDataReader;

public class AdtDbCommand : DbCommand {
    public override string? CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction DbTransaction { get; set; }

    public override void Cancel() {
        throw new NotImplementedException();
    }

    public override int ExecuteNonQuery() {
        throw new NotImplementedException();
    }

    public override object ExecuteScalar() {
        throw new NotImplementedException();
    }

    public override void Prepare() {
        throw new NotImplementedException();
    }

    protected override DbParameter CreateDbParameter() {
        throw new NotImplementedException();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) {
        var AdtDbConnection = Connection as AdtDbConnection;
        if (AdtDbConnection is null) {
            throw new InvalidOperationException($"{nameof(AdtDbConnection)} is not available");
        }

        var folder = AdtDbConnection.Database;
        if (string.IsNullOrWhiteSpace(folder)) {
            throw new DirectoryNotFoundException("No folder was specified for the Adt files.");
        }

        if (!Directory.Exists(folder)) {
            throw new DirectoryNotFoundException($"The specified folder does not exist: {folder}");
        }

        var TableName = QueryParser.Parse(CommandText);

        var (DataFilePath, MemoFilePath) = GetFilePath(folder, TableName);

        var options = AdtDbConnection.Options;
        return new AdtDataReader(DataFilePath, MemoFilePath, options);
    }

    private static (string, string?) GetFilePath(string Folder, string TableName) {
        
        var DataFilePath = default(string?);
        var MemoFilePath = default(string?);

        var Sets = new[] {
            ($"adt", $"adm"),
            ($"add", $"am"),
        };

        foreach(var (DataExt, MemoExt) in Sets) {
            var Adt = Path.Combine(Folder, $"{TableName}.{DataExt}");
            var Adm = Path.Combine(Folder, $"{TableName}.{MemoExt}");

            if (File.Exists(Adt)) {
                DataFilePath = Adt;

                if (File.Exists(Adm)) {
                    MemoFilePath = Adm;
                }

                break;
            }

        }

        if(DataFilePath == null) {
            var Adt = Path.Combine(Folder, $"{TableName}.adt");
            throw new FileNotFoundException(Adt);
        }

        return (DataFilePath, MemoFilePath);

    }
}
