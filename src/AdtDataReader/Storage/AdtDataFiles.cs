using Microsoft.Win32.SafeHandles;
using System.Text;

namespace AdtDataReader.Storage;

public static class AdtDataFiles {

    static AdtDataFiles() {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static AdtDataFile OpenRead(string FullPath) {

        var DefaultEncoding = 437;

        return OpenRead(FullPath, DefaultEncoding);
    }

    public static AdtDataFile OpenRead(string FullPath, int FileEncoding) {
        var Encoding = System.Text.Encoding.GetEncoding(FileEncoding);
        return OpenRead(FullPath, Encoding);
    }

    public static AdtDataFile OpenRead(string FullPath, System.Text.Encoding FileEncoding) {
        var RecordStream = File.OpenHandle(FullPath, share: FileShare.ReadWrite);
        
        return OpenRead(RecordStream, FileEncoding);
    }

    public static AdtDataFile OpenRead(SafeFileHandle Reader, System.Text.Encoding FileEncoding) {
        
        var Header = AdtDataFileReader.ReadHeader(Reader);
        var Columns = new List<AdtDataFileColumn>();
        
        for(uint i = 0; i < Header.ColumnCount; i++) {
            var Column = AdtDataFileReader.ReadColumn(Reader, i);
            Columns.Add(Column);
        }

        Columns = Columns
            .OrderBy(x => x.Offset)
            .ToList()
            ;

        var ret = new AdtDataFile(Reader, FileEncoding, Header, [..Columns]);
        return ret;
    }
}