using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;

namespace AdtDataReader.Storage;

public static class AdtMemoFileReader {
    public static AdtMemoFileHeader ReadHeader(SafeFileHandle Input) {

        var NextFreeBlockBuffer = new byte[sizeof(uint)];
        var NextFreeBlockLocation = 0;

        RandomAccess.Read(Input, NextFreeBlockBuffer, NextFreeBlockLocation);
        var NextFreeBlock = BinaryPrimitives.ReadUInt32BigEndian(NextFreeBlockBuffer);

        var BlockSizeBuffer = new byte[sizeof(ushort)];
        var BlockSizeLocation = 6;

        RandomAccess.Read(Input, BlockSizeBuffer, BlockSizeLocation);
        var BlockSize = BinaryPrimitives.ReadUInt16BigEndian(BlockSizeBuffer);

        var ret = new AdtMemoFileHeader() {
            NextFreeBlock = NextFreeBlock,
            BlockSize = BlockSize,
        };

        return ret;
    }
}