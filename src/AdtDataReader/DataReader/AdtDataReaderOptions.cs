using System.Text;

namespace AdtDataReader;

public class AdtDataReaderOptions {
    public AdtDataReaderOptions() {
        SkipDeletedRecords = false;
        Encoding = null;
        StringTrimming = StringTrimmingOption.Trim;
        ReadFloatsAsDecimals = false;
    }

    public bool SkipDeletedRecords { get; set; }
    public Encoding Encoding { get; set; }
    public StringTrimmingOption StringTrimming { get; set; }
    public bool ReadFloatsAsDecimals { get; set; }
}
