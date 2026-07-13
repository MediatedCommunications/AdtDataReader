using Microsoft.Win32.SafeHandles;

namespace AdtDataReader.Storage;

public class AdtMemoFile : IDisposable {
    private bool disposedValue;

    private SafeFileHandle Reader { get; }
    public AdtMemoFileHeader Header { get; }

    internal AdtMemoFile(SafeFileHandle Reader, AdtMemoFileHeader Header) {
        this.Reader = Reader;
        this.Header = Header;
    }

    public byte[] ReadBytes(int BlockNumber, int Length) {
        var ret = Array.Empty<byte>();

        if(Length > 0) {
            ret = new byte[Length];

            RandomAccess.Read(Reader, ret, BlockNumber * Header.BlockSize);
        }

        return ret;
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                Reader.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
