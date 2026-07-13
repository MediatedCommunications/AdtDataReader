namespace AdtDataReader.Storage;

public static class AdtMemoFiles {
    public static AdtMemoFile OpenRead(string FullPath) {
        var Reader = File.OpenHandle(FullPath);
        var Header = AdtMemoFileReader.ReadHeader(Reader);

        var ret = new AdtMemoFile(Reader, Header);

        return ret;
    }
}
