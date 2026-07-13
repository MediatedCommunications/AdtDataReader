using System.Data;
using System.Data.Common;
using System.Text;

namespace AdtDataReader;

public class AdtDbConnection : DbConnection {
    private string _database = string.Empty;
    private ConnectionState _state = ConnectionState.Closed;

    public AdtDbConnection() {
        DataSource = string.Empty;
        ServerVersion = string.Empty;
    }

    public AdtDbConnection(string dataSource, string serverVersion) {
        DataSource = dataSource;
        ServerVersion = serverVersion;
    }

    public AdtDataReaderOptions Options { get; private set; } = new AdtDataReaderOptions();

    public override string ConnectionString { get; set; }
    public override string DataSource { get; }
    public override string ServerVersion { get; }

    public override ConnectionState State => _state;
    public override string Database => _database;

    public override void ChangeDatabase(string databaseName) {
        if (!Directory.Exists(databaseName)) {
            throw new DirectoryNotFoundException(databaseName);
        }

        _database = databaseName;
    }

    public override void Close() {
        _state = ConnectionState.Closed;
    }

    public override void Open() {
        var builder = new AdtDbConnectionStringBuilder(ConnectionString);

        ChangeDatabase(builder.Folder);

        var options = new AdtDataReaderOptions();

        if (builder.Encoding is { } encoding) {
            options.Encoding = Encoding.GetEncoding(encoding);
        }

        if (builder.SkipDeletedRecords is var skipDeletedRecords) {
            options.SkipDeletedRecords = skipDeletedRecords;
        }

        if (builder.StringTrimming is var stringTrimming) {
            options.StringTrimming = stringTrimming;
        }

        Options = options;
        _state = ConnectionState.Open;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
        throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand() {
        return new AdtDbCommand {
            Connection = this,
        };
    }
}
