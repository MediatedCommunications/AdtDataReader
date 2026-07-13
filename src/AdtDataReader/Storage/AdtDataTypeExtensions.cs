using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text;

namespace AdtDataReader.Storage;

public static class AdtDataTypeExtensions {

    public static Type GetCrlTypeWithMemo(this AdtDataType This) {
        var ret = This switch {
            AdtDataType.Memo => typeof(string),
            AdtDataType.NMemo => typeof(string),
            AdtDataType.Binary => typeof(byte[]),
            AdtDataType.Image => typeof(byte[]),
            AdtDataType.VarBinaryFox => typeof(byte[]),

            _ => GetCrlTypeWithoutMemo(This),
        };

        return ret;
    }


    public static Type GetCrlTypeWithoutMemo(this AdtDataType This) {
        var ret = This switch {
            AdtDataType.Logical => typeof(bool?),
            AdtDataType.Numeric => typeof(double?),
            AdtDataType.Date => typeof(DateOnly?),
            AdtDataType.CompactDate => typeof(DateOnly?),
            AdtDataType.Character => typeof(string),
            AdtDataType.CiCharacter => typeof(string),

            AdtDataType.Memo => typeof(AdtMemoFileReference),
            AdtDataType.NMemo => typeof(AdtMemoFileReference),
            AdtDataType.Binary => typeof(AdtMemoFileReference),
            AdtDataType.Image => typeof(AdtMemoFileReference),
            AdtDataType.VarBinaryFox => typeof(AdtMemoFileReference),

            AdtDataType.VarCharFox => typeof(string),

            AdtDataType.Guid => typeof(Guid),

            AdtDataType.Double => typeof(double?),

            AdtDataType.Integer => typeof(int?),
            AdtDataType.AutoInc => typeof(int?),
            AdtDataType.Long => typeof(long?),
            AdtDataType.Short => typeof(short?),
            AdtDataType.Time => typeof(TimeOnly?),
            AdtDataType.TimeStamp => typeof(DateTime?),
            AdtDataType.ModTime => typeof(DateTime?),
            AdtDataType.Raw => typeof(byte[]),
            AdtDataType.Currency => typeof(double),
            AdtDataType.Money => typeof(decimal),
            AdtDataType.RowVersion => typeof(long),
            AdtDataType.VarChar => typeof(string),

            AdtDataType.NChar => typeof(string),
            AdtDataType.NVarChar => typeof(string),

            _ => throw new InvalidDataTypeException(This),
        };

        return ret;
    }
}

public class InvalidDataTypeException : Exception {
    
    private static string GetMessage(AdtDataType Input) {
        var ret = $@"Unexpected {nameof(AdtDataType)} Value: {Input}";
        return ret;
    }

    public InvalidDataTypeException(AdtDataType Type) : base(GetMessage(Type)) {

    }
}