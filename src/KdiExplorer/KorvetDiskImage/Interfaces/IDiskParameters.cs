namespace KorvetDiskImage.Interfaces
{
    public interface IDiskParameters
    {

        // DISK PARAMETER BLOCK
        public ushort SPT { get; }  //  Number of 128-byte records per track (5 phys sect * 1024 bytes / 128 bytes)
        public byte BSH { get; }  //  Block shift. 3 => 1k, 4 => 2k, 5 => 4k....
        public byte BLM { get; }  //  Block mask. 7 => 1k, 0Fh => 2k, 1Fh => 4k...
        public byte EXM { get; }  //  Extent mask, see later
        public ushort DSM { get; }  // (no.of blocks on the disc)-1
        public ushort DRM { get; }  //  (no.of directory entries)-1
        public byte AL0 { get; }  //  Directory allocation bitmap, first byte
        public byte AL1 { get; }  // Directory allocation bitmap, second byte
        public ushort CKS { get; }   // Checksum vector size, 0 for a fixed disc, No.directory entries/4, rounded up.
        public ushort OFF { get; }   //  Offset, number of reserved tracks

    }
}
