using AdtDataReader;
using AdtDataReader.Storage;

var C = new AdtDbConnectionStringBuilder();
C.Folder = $@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage\";

var Connection = new AdtDbConnection();
Connection.ConnectionString = C.ConnectionString;
Connection.Open();

var Command = Connection.CreateCommand();
Command.CommandText = "SELECT * FROM RolodexQ";

using var DR = Command.ExecuteReader();
while (DR.Read()) {

}



var Sources = new[] {
    //($@"X:\TEMP\A\Database\®.adt", $@"X:\TEMP\A\Database\Test.am"),
    //($@"X:\TEMP\A\Database\Integer.adt", $@"X:\TEMP\A\Database\Test.am"),
    ($@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage\RolodexQ.adt", $@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage\RolodexQ.adm"),
    //($@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage\Prevail.add", $@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage\Prevail.am"),
};

foreach (var (DataFilePath, MemoFilePath) in Sources) {

    
    {
        using var DataFile = AdtDataFiles.OpenRead(DataFilePath);
        using var MemoFile = AdtMemoFiles.OpenRead(MemoFilePath);


        foreach (var Row in DataFile.ReadRows()) {
            var Values = new Dictionary<AdtDataFileColumn, object?>();

            for (var i = 0; i < Row.Values.Length; i++) {

                var Value = Row.Values[i];

                if (Value is AdtMemoFileReference { } Reference) {
                    Value = MemoFile.ReadValue(Reference.BlockNumber, Reference.Length, DataFile.Columns[i].DataType, DataFile.Encoding);
                }
                Values[DataFile.Columns[i]] = Value;

            }

            if (!Row.IsDeleted) {

            }
        }
    }

    try {
        //var Reader = new NativeAdt.AdtReader(Source, "");
        //while (Reader.ReadHeader()) { 
        //
        //}

    } catch (Exception ex) {

    }

}

var Files = Directory.GetFiles($@"X:\TEMP\LEGACY_Prevail_TestDb4.SybaseAdvantage\LEGACY_Prevail_TestDb4.SybaseAdvantage", "*.adt");

foreach (var Source in Files) {
    Console.WriteLine(Source);

    if (true) {
        using var DataFile = AdtDataFiles.OpenRead(Source);


        foreach(var Row in DataFile.ReadRows()) {

        }

    }

}

Console.WriteLine();