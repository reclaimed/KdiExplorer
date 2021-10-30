using System.Text;


using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage
{
    public class DirectoryEntry
    {
        public byte UserNumber { get; private set; }
        public string Filename { get; private set; }
        public string Filetype { get; private set; }

        public int EntryNumber { get; private set; } // Entry number = ((32*S2)+EX) / (exm+1)
        public int NumberOfRecords { get; private set; } // (EX & exm) * 128 + RC
        public List<int> Allocation { get; private set; } = new List<int>();

        public bool IsReadOnly = false;
        public bool IsHidden = false;


        public DirectoryEntry(ByteStorage entry, IDiskParameters dpb)
        {

            // name
            UserNumber = entry.GetByte(0);
            Filename = Encoding.ASCII.GetString(entry.GetRange(1, 8).AsArray);

            // clear attribute bits
            var ftyp = entry.GetRange(9, 3).AsArray;
            ftyp[0] = (byte)(ftyp[0] & 127);
            ftyp[1] = (byte)(ftyp[1] & 127);
            Filetype = Encoding.ASCII.GetString(ftyp);

            // attributes
            IsReadOnly = (entry.GetByte(9) & 128) == 128; // T1'
            IsHidden = (entry.GetByte(10) & 128) == 128; // T2'



            // AL values:
            //  disk_blocks_number < 256: 8-bit
            //  disk_blocks_number >=256: 16-bit (stored low byte first)
            // DPB.DSM: disk_blocks_number-1
            if (dpb.DSM > 255)
            {
                // 16-bit AL numbers
                for (int i = 16; i < 31; i += 2)
                {
                    var clus = entry.GetByte(i + 1) << 8 | entry.GetByte(i);

                    if (clus != 0)
                    {
                        Allocation.Add(clus);
                    }
                }
            }
            else
            {
                // 8-bit AL numbers
                foreach (int al in entry.GetRange(16, 16).AsList)
                {
                    if (al != 0)
                    {
                        Allocation.Add(al);
                    }
                }
            }

            // properties
            var EX = entry.GetByte(12);
            var S1 = entry.GetByte(13);
            var S2 = entry.GetByte(14);
            var RC = entry.GetByte(15);

            EntryNumber = ((32 * S2) + EX) / (dpb.EXM + 1);
            NumberOfRecords = (EX & dpb.EXM) * 128 + RC;



        }




        /*
        The CP/M 2.2 directory has only one type of entry:

        UU F1 F2 F3 F4 F5 F6 F7 F8 T1 T2 T3 EX S1 S2 RC   .FILENAMETYP....
        AL AL AL AL AL AL AL AL AL AL AL AL AL AL AL AL   ................

        UU = User number. 0-15 (on some systems, 0-31). The user number allows multiple
            files of the same name to coexist on the disc. 
             User number = 0E5h => File deleted
        Fn - filename
        Tn - filetype. The characters used for these are 7-bit ASCII.
               The top bit of T1 (often referred to as T1') is set if the file is 
             read-only.
               T2' is set if the file is a system file (this corresponds to "hidden" on 
             other systems). 
        EX = Extent counter, low byte - takes values from 0-31
        S2 = Extent counter, high byte.

              An extent is the portion of a file controlled by one directory entry.
            If a file takes up more blocks than can be listed in one directory entry,
            it is given multiple entries, distinguished by their EX and S2 bytes. The
            formula is: Entry number = ((32*S2)+EX) / (exm+1) where exm is the 
            extent mask value from the Disc Parameter Block.

        S1 - reserved, set to 0.
        RC - Number of records (1 record=128 bytes) used in this extent, low byte.
            The total number of records used in this extent is

            (EX & exm) * 128 + RC

            If RC is 80h, this extent is full and there may be another one on the disc.
            File lengths are only saved to the nearest 128 bytes.

        AL - Allocation. Each AL is the number of a block on the disc. If an AL
            number is zero, that section of the file has no storage allocated to it
            (ie it does not exist). For example, a 3k file might have allocation 
            5,6,8,0,0.... - the first 1k is in block 5, the second in block 6, the 
            third in block 8.
             AL numbers can either be 8-bit (if there are fewer than 256 blocks on the
            disc) or 16-bit (stored low byte first). 

        */



    }
}
