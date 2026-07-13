using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.Text;

namespace AdtDataReader.Storage;

public static partial class AdtDataFileReader {
    public static uint HeaderBlockSize { get; } = 400;
    public static uint ColumnBlockSize { get; } = 200;

    public static AdtDataFileHeader ReadHeader(SafeFileHandle Reader) {

        var HeaderBlockBytes = new byte[HeaderBlockSize];
        var HeaderBlock = HeaderBlockBytes.AsSpan();
        RandomAccess.Read(Reader, HeaderBlock, 0);

        var TypeName = Encoding.UTF8.GetString(HeaderBlock[0..20]);
        TypeName = TypeName.TrimEndNulls().TrimEnd();

        if (TypeName != "Advantage Table" && TypeName != "ADS Data Dictionary") {
            throw new Exception("Invalid Advantage table file");
        }

        var RowCount = BinaryPrimitives.ReadUInt32LittleEndian(HeaderBlock[24..]);
        var RowOffset = BinaryPrimitives.ReadUInt32LittleEndian(HeaderBlock[32..]);
        var ColumnCount = (RowOffset - HeaderBlockSize) / ColumnBlockSize;
        var RowSize = BinaryPrimitives.ReadUInt16LittleEndian(HeaderBlock[36..]);

    
        var DictionaryFileNameBytes = HeaderBlock[92..][..260];
        var DictionaryFileName = Encoding.UTF8.GetString(DictionaryFileNameBytes);
        DictionaryFileName = DictionaryFileName.TrimEndNulls().TrimEnd();
    
        var ret = new AdtDataFileHeader() {
            RowCount = RowCount,
            RowOffset = RowOffset,
            ColumnCount = ColumnCount,
            RowSize = RowSize,
            DictionaryFileName = DictionaryFileName,
        };
    
        return ret;
    }

    public static AdtDataFileColumn ReadColumn(SafeFileHandle Reader, uint Index) {

        var ColumnBlockBytes = new byte[ColumnBlockSize];
        var ColumnBlock = ColumnBlockBytes.AsSpan();

        RandomAccess.Read(Reader, ColumnBlock, HeaderBlockSize + (Index * ColumnBlockSize));

        var Name = Encoding.UTF8.GetString(ColumnBlock[0..][..128])
            .TrimEndNulls()
            .TrimEnd()
            ;

        var Type = (AdtDataType)ColumnBlock[129];
        
        var Offset = BinaryPrimitives.ReadInt32LittleEndian(ColumnBlock[131..]);
        var Length = (int) BinaryPrimitives.ReadUInt16LittleEndian(ColumnBlock[135..]);
        if (Length == 0 && Type == AdtDataType.Character) {
            Length = 1048576;
        }
        var Scale = ColumnBlock[139];

        var ret = new AdtDataFileColumn() {
            Name = Name,
            DataType = Type,
            Offset = Offset,
            Length = Length,
            Scale = Scale,
        };

        return ret;
    }

   
}
