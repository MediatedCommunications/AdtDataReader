using System.Globalization;
using System.Text;

namespace AdtDataReader.Storage;

public static partial class AdtDataTypeParser {

    public static object? Parse(AdtDataType DataType, ReadOnlySpan<byte> Bytes, Encoding Encoding) {
        var ret = default(object?);

        try {
            ret = DataType switch {
                AdtDataType.Logical => ParseLogical(Bytes),
                AdtDataType.Numeric => ParseNumeric(Bytes),
                AdtDataType.Date => ParseDate(Bytes),
                AdtDataType.CompactDate => ParseCompactDate(Bytes),
                AdtDataType.Character => ParseString(Bytes, Encoding),
                AdtDataType.CiCharacter => ParseString(Bytes, Encoding),

                AdtDataType.Memo => ParseMemoSegment(Bytes),
                AdtDataType.NMemo => ParseMemoSegment(Bytes),
                AdtDataType.Binary => ParseMemoSegment(Bytes),
                AdtDataType.Image => ParseMemoSegment(Bytes),
                AdtDataType.VarBinaryFox => ParseMemoSegment(Bytes),

                AdtDataType.VarCharFox => ParseString(Bytes, Encoding),

                AdtDataType.Guid => ParseGuid(Bytes),

                AdtDataType.Double => ParseDouble(Bytes),

                AdtDataType.Integer => ParseInteger(Bytes),
                AdtDataType.AutoInc => ParseInteger(Bytes),
                AdtDataType.Long => ParseLong(Bytes),
                AdtDataType.Short => ParseShort(Bytes),
                AdtDataType.Time => ParseTime(Bytes),
                AdtDataType.TimeStamp => ParseDateTime(Bytes),
                AdtDataType.ModTime => ParseDateTime(Bytes),
                AdtDataType.Raw => ParseRaw(Bytes),
                AdtDataType.Currency => ParseCurrency(Bytes),
                AdtDataType.Money => ParseMoney(Bytes),
                AdtDataType.RowVersion => ParseRowVersion(Bytes),
                AdtDataType.VarChar => ParseVarChar(Bytes, Encoding),

                AdtDataType.NChar => ParseUnicodeString(Bytes).TrimEnd(),
                AdtDataType.NVarChar => ParseUnicodeString(Bytes).TrimEnd(),

                _ => new InvalidDataTypeException(DataType),
            };

        } catch (Exception ex) {
            ret = ex;
        }

        return ret;
    }

    public static AdtMemoFileReference ParseMemoSegment(ReadOnlySpan<byte> Bytes) {

        var BlockNumber = BitConverter.ToInt32(Bytes[0..]);
        var BlockSize = BitConverter.ToInt32(Bytes[4..]);

        var ret = new AdtMemoFileReference() {
            BlockNumber = BlockNumber,
            Length = BlockSize,
        };

        return ret;
    }

    public static string ParseEncodedString(ReadOnlySpan<byte> Bytes, Encoding Encoding) {
        var ret = Encoding.GetString(Bytes)
            .TrimEndNulls()
            ;

        return ret;
    }

    public static string ParseUnicodeString(ReadOnlySpan<byte> Bytes) {
        var ret = Encoding.Unicode.GetString(Bytes)
            .TrimEndNulls()
            ;
        return ret;
    }


    public static bool? ParseLogical(ReadOnlySpan<byte> Bytes) {
        var ret = Bytes[0] switch {
            (byte)'T' => true,
            (byte)'F' => false,
            _ => default(bool?),
        };
        return ret;
    }

    public static double? ParseNumeric(ReadOnlySpan<byte> Bytes) {
        var ret = default(double?);

        var Value = Encoding.UTF8.GetString(Bytes)
            .TrimEndNulls()
            .Trim()
            ;

        if (!string.IsNullOrWhiteSpace(Value)) {
            //This might need some adjustment based on values.
            ret = double.Parse(Value, CultureInfo.InvariantCulture);
        }

        return ret;
    }

    public static DateOnly? ParseDate(ReadOnlySpan<byte> Bytes) {
        var ret = default(DateOnly?);

        var Days = BitConverter.ToInt32(Bytes);

        if (Days != 0) {
            ret = new DateOnly(1970, 1, 1).AddDays(Days - 2440588);
        }

        return ret;
    }

    public static DateOnly? ParseCompactDate(ReadOnlySpan<byte> Bytes) {
        var ret = default(DateOnly?);

        var Y = Bytes[0];
        var M = Bytes[1];
        var D = Bytes[2];

        if (Y != 0 || M != 0 || D != 0) {
            ret = new DateOnly(1900 + Y, M, D);
        }

        return ret;
    }

    public static string? ParseString(ReadOnlySpan<byte> Bytes, Encoding Encoding) {
        var ret = default(string?);

        var Value = ParseEncodedString(Bytes, Encoding)
            .TrimEndNulls()
            .TrimEnd()
            ;

        if (!string.IsNullOrWhiteSpace(Value)) {
            ret = Value;
        }

        return ret;
    }

    public static double? ParseDouble(ReadOnlySpan<byte> Bytes) {
        var ret = default(double?);

        var marker = Encoding.UTF8.GetString(Bytes);

        if (marker != "\x2000000000000080") {
            ret = BitConverter.ToDouble(Bytes);
        }

        return ret;
    }

    public static int? ParseInteger(ReadOnlySpan<byte> Bytes) {
        var ret = default(int?);

        var tret = BitConverter.ToInt32(Bytes);
        if (tret != int.MinValue) {
            ret = tret;
        }

        return ret;
    }

    public static long? ParseLong(ReadOnlySpan<byte> Bytes) {
        var ret = default(long?);

        var tret = BitConverter.ToInt64(Bytes);
        if (tret != long.MinValue) {
            ret = tret;
        }

        return ret;
    }

    public static Guid ParseGuid(ReadOnlySpan<byte> Bytes) {
        var ret = Guid.Parse(Bytes);

        return ret;
    }

    public static short? ParseShort(ReadOnlySpan<byte> Bytes) {
        var ret = default(short?);

        var tret = BitConverter.ToInt16(Bytes);

        if (tret != short.MinValue) {
            ret = tret;
        }

        return ret;
    }

    public static TimeOnly? ParseTime(ReadOnlySpan<byte> Bytes) {
        var ret = default(TimeOnly?);
        var MS = BitConverter.ToInt32(Bytes);

        if (MS != -1) {
            var TS = TimeSpan.FromMilliseconds(MS);
            ret = TimeOnly.FromTimeSpan(TS);
        }

        return ret;
    }

    public static DateTime? ParseDateTime(ReadOnlySpan<byte> Bytes) {
        var ret = default(DateTime?);

        var DatePart = ParseDate(Bytes[0..]) ?? DateOnly.MinValue;
        var TimePart = ParseTime(Bytes[4..]) ?? TimeOnly.MinValue;

        if (DatePart is { } Date && TimePart is { } Time) {
            ret = Date.ToDateTime(Time);
        }

        return ret;
    }

    public static byte[] ParseRaw(ReadOnlySpan<byte> Bytes) {

        var ret = Bytes.ToArray();

        return ret;
    }

    public static double ParseCurrency(ReadOnlySpan<byte> Bytes) {
        var ret = BitConverter.ToDouble(Bytes);
        return ret;
    }

    public static decimal? ParseMoney(ReadOnlySpan<byte> Bytes) {
        var ret = default(decimal?);

        var tret = BitConverter.ToInt64(Bytes);
        if (tret != long.MinValue) {
            ret = tret / 10000m;
        }

        return ret;
    }

    public static long ParseRowVersion(ReadOnlySpan<byte> Bytes) {
        var ret = BitConverter.ToInt64(Bytes);
        return ret;
    }

    public static string? ParseVarChar(ReadOnlySpan<byte> Bytes, Encoding Encoding) {
        var ret = default(string?);
        var tret = ParseEncodedString(Bytes, Encoding);
        var NullIndex = tret.IndexOf('\0');
        if (NullIndex >= 0) {
            tret = tret[..NullIndex];
        }

        if (!string.IsNullOrWhiteSpace(tret)) {
            ret = tret;
        }

        return ret;
    }
}
