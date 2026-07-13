using System.Data.Common;
using System.Diagnostics;

namespace AdtDataReader;

public class AdtDbConnectionStringBuilder : DbConnectionStringBuilder {
    private enum Keywords {
        Encoding,
        Folder,
        SkipDeletedRecords,
        StringTrimming,

        // keep the count value last
        KeywordsCount
    }

    internal const int KeywordsCount = (int)Keywords.KeywordsCount;

    private static readonly string[] ValidKeywords = BuildValidKeywords();
    private static readonly Dictionary<string, Keywords> KeywordsHash = BuildKeywordsHash();

    private string _encoding;
    private string _folder = string.Empty;
    private bool _readFloatsAsDecimals;
    private bool _skipDeletedRecords = true;
    private StringTrimmingOption _stringTrimming = StringTrimmingOption.Trim;

    public AdtDbConnectionStringBuilder() : this(null) {
    }

    public AdtDbConnectionStringBuilder(string connectionString) {
        if (!string.IsNullOrWhiteSpace(connectionString)) {
            ConnectionString = connectionString;
        }
    }

    public override object this[string keyword] {
        get {
            Keywords index = GetIndex(keyword);
            return GetAt(index);
        }
        set {
            if (null != value) {
                Keywords index = GetIndex(keyword);
                switch (index) {
                    case Keywords.Encoding: Encoding = AdtDbConnectionStringBuilderUtil.ConvertToString(value); break;
                    case Keywords.Folder: Folder = AdtDbConnectionStringBuilderUtil.ConvertToString(value); break;
                    case Keywords.SkipDeletedRecords: SkipDeletedRecords = AdtDbConnectionStringBuilderUtil.ConvertToBoolean(value); break;
                    case Keywords.StringTrimming: StringTrimming = AdtDbConnectionStringBuilderUtil.ConvertToStringTrimmingOption(keyword, value); break;
                    default:
                        Debug.Assert(false, "unexpected keyword");
                        throw AdtDbConnectionStringBuilderUtil.KeywordNotSupported(keyword);
                }
            } else {
                Remove(keyword);
            }
        }
    }

    public string Encoding {
        get => _encoding;
        set {
            SetValue(AdtDbConnectionStringKeywords.Encoding, value);
            _encoding = value;
        }
    }

    public string Folder {
        get => _folder;
        set {
            SetValue(AdtDbConnectionStringKeywords.Folder, value);
            _folder = value;
        }
    }

    public bool SkipDeletedRecords {
        get => _skipDeletedRecords;
        set {
            SetValue(AdtDbConnectionStringKeywords.SkipDeletedRecords, value);
            _skipDeletedRecords = value;
        }
    }

    public StringTrimmingOption StringTrimming {
        get => _stringTrimming;
        set {
            if (!AdtDbConnectionStringBuilderUtil.IsValidStringTrimmingOptionValue(value)) {
                throw AdtDbConnectionStringBuilderUtil.InvalidEnumerationValue(typeof(StringTrimmingOption), (int)value);
            }

            SetStringTrimmingValue(value);
            _stringTrimming = value;
        }
    }

    public override bool Remove(string keyword) {
        AdtDbConnectionStringBuilderUtil.CheckArgumentNull(keyword, "keyword");
        if (!KeywordsHash.TryGetValue(keyword, out var index) ||
            !base.Remove(ValidKeywords[(int)index])) {
            return false;
        }

        Reset(index);
        return true;
    }

    public override bool TryGetValue(string keyword, out object value) {
        if (KeywordsHash.TryGetValue(keyword, out var index)) {
            value = GetAt(index);
            return true;
        }

        value = null;
        return false;
    }

    private void SetValue(string keyword, bool value) {
        base[keyword] = value.ToString();
    }

    private void SetValue(string keyword, string value) {
        AdtDbConnectionStringBuilderUtil.CheckArgumentNull(value, keyword);
        base[keyword] = value;
    }

    private void SetStringTrimmingValue(StringTrimmingOption value) {
        Debug.Assert(AdtDbConnectionStringBuilderUtil.IsValidStringTrimmingOptionValue(value), "Invalid value for StringTrimming");
        base[AdtDbConnectionStringKeywords.StringTrimming] = AdtDbConnectionStringBuilderUtil.StringTrimmingOptionToString(value);
    }

    private object GetAt(Keywords index) {
        switch (index) {
            case Keywords.Encoding: return Encoding;
            case Keywords.Folder: return Folder;
            case Keywords.SkipDeletedRecords: return SkipDeletedRecords;
            case Keywords.StringTrimming: return StringTrimming;
            default:
                Debug.Assert(false, "unexpected keyword");
                throw AdtDbConnectionStringBuilderUtil.KeywordNotSupported(ValidKeywords[(int)index]);
        }
    }

    private static Keywords GetIndex(string keyword) {
        AdtDbConnectionStringBuilderUtil.CheckArgumentNull(keyword, "keyword");
        if (KeywordsHash.TryGetValue(keyword, out var index)) {
            return index;
        }
        throw AdtDbConnectionStringBuilderUtil.KeywordNotSupported(keyword);
    }

    private void Reset(Keywords index) {
        switch (index) {
            case Keywords.Encoding:
                _encoding = string.Empty;
                break;
            case Keywords.Folder:
                _folder = string.Empty;
                break;
            case Keywords.SkipDeletedRecords:
                _skipDeletedRecords = true;
                break;
            case Keywords.StringTrimming:
                _stringTrimming = StringTrimmingOption.Trim;
                break;
            default:
                Debug.Assert(false, "unexpected keyword");
                throw AdtDbConnectionStringBuilderUtil.KeywordNotSupported(ValidKeywords[(int)index]);
        }
    }

    private static string[] BuildValidKeywords() {
        var validKeywords = new string[KeywordsCount];
        validKeywords[(int)Keywords.Encoding] = AdtDbConnectionStringKeywords.Encoding;
        validKeywords[(int)Keywords.Folder] = AdtDbConnectionStringKeywords.Folder;
        validKeywords[(int)Keywords.SkipDeletedRecords] = AdtDbConnectionStringKeywords.SkipDeletedRecords;
        validKeywords[(int)Keywords.StringTrimming] = AdtDbConnectionStringKeywords.StringTrimming;
        return validKeywords;
    }

    private static Dictionary<string, Keywords> BuildKeywordsHash() {
        var hash = new Dictionary<string, Keywords>(KeywordsCount, StringComparer.OrdinalIgnoreCase)
        {
                { AdtDbConnectionStringKeywords.Encoding, Keywords.Encoding },
                { AdtDbConnectionStringKeywords.Folder, Keywords.Folder },
                { AdtDbConnectionStringKeywords.SkipDeletedRecords, Keywords.SkipDeletedRecords },
                { AdtDbConnectionStringKeywords.StringTrimming, Keywords.StringTrimming }
            };
        Debug.Assert(KeywordsCount == hash.Count, "initial expected size is incorrect");
        return hash;
    }
}

