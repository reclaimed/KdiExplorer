
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage.Cpm22
{
    public class GenericDPB : IDiskParameters
    {

        // DISK PARAMETER BLOCK
        public ushort SPT { get; private set; } = 40;  //  Number of 128-byte records per track (5 phys sect * 1024 bytes / 128 bytes)
        public byte BSH { get; private set; } = 3;  //  Block shift. 3 => 1k, 4 => 2k, 5 => 4k....
        public byte BLM { get; private set; } = 7;  //  Block mask. 7 => 1k, 0Fh => 2k, 1Fh => 4k...
        public byte EXM { get; private set; } = 0;  //  Extent mask, see later
        public ushort DSM { get; private set; } = 99;  // (no.of blocks on the disc)-1

        public ushort DRM { get; private set; } = 63;  //  (no.of directory entries)-1

        public byte AL0 { get; private set; } = 192;  //  Directory allocation bitmap, first byte
        public byte AL1 { get; private set; } = 0;  // Directory allocation bitmap, second byte
        public ushort CKS { get; private set; } = 8;   // Checksum vector size, 0 for a fixed disc, No.directory entries/4, rounded up.
        public ushort OFF { get; private set; } = 0;   //  Offset, number of reserved tracks


        public GenericDPB() { }

        public GenericDPB(ushort spt, byte bsh, byte blm, byte exm, ushort dsm, ushort drm, byte al0, byte al1, ushort cks, ushort off)
        {

            SPT = spt;
            BSH = bsh;
            BLM = blm;
            EXM = exm;
            DSM = dsm;
            DRM = drm;
            AL0 = al0;
            AL1 = al1;
            CKS = cks;
            OFF = off;

        }



    }
}
