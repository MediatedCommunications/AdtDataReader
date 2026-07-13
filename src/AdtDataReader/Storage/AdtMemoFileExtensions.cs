using System.Text;

namespace AdtDataReader.Storage;

public static class AdtMemoFileExtensions {

    public static object ReadValue(this AdtMemoFile This, AdtMemoFileReference Reference, AdtDataFileColumn Column, Encoding FileEncoding) {
        var ret = ReadValue(This, Reference.BlockNumber, Reference.Length, Column.DataType, FileEncoding);
        return ret;
    }

    public static object ReadValue(this AdtMemoFile This, int BlockNumber, int Length, AdtDataType ColumnType, Encoding FileEncoding) {
        object ret = ColumnType switch {
            AdtDataType.Binary => This.ReadBytes(BlockNumber, Length),
            AdtDataType.Image => This.ReadBytes(BlockNumber, Length),
            AdtDataType.VarBinaryFox => This.ReadBytes(BlockNumber, Length),
            AdtDataType.Memo => This.ReadString(BlockNumber, Length, FileEncoding),
            AdtDataType.NMemo => This.ReadUnicode(BlockNumber, Length),
            _ => throw new NotImplementedException(),
        };
        return ret;
    }

    public static string ReadString(this AdtMemoFile This, int BlockNumber, int Length, Encoding Encoding) {
        var Bytes = This.ReadBytes(BlockNumber, Length);
        var ret = Encoding.GetString(Bytes).TrimEndNulls();

        return ret;
    }

    public static string ReadUnicode(this AdtMemoFile This, int BlockNumber, int Length) {
        return This.ReadString(BlockNumber, Length, Encoding.Unicode);
    }

}
