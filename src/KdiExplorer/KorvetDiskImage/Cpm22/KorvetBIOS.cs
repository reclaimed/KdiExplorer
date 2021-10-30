
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage.Cpm22
{

    // Translate CP/M calls to real hardware.

    public class KorvetBIOS : IBlockDevice
    {

        private int RecordSize = 128;


        private ByteStorage Media = new ByteStorage();
        private IDiskParameters DPB = new GenericDPB();
        private bool MediaChanged = false;
        private int Track;
        private int Sector;


        // KORVET-SPECIFIC DISK PARAMETER HEADER
        private byte SecSize;           //	(3)  размер физического сектора:0 - 128 байт, 1 - 256 байт, 2 - 512 байт и  3 - 1024
        private byte InSide;            //	(1)  информация о сторонах диска:
                                        //	        0 - односторонний диск,
                                        //	        1 - двухсторонний,
                                        //	     сектора нумеруются от 1 до n с каждой стороны, четные логические
                                        //	     дорожки с нулевой стороны, нечетные с первой
                                        //
        private byte SecPerTrack;       //	(5)  число физических секторов на дорожке
        private byte TrkPerDisk;        //	(80) число дорожек на диске (с одной стороны)

        public KorvetBIOS(ByteStorage virtual_media)
        {
            INSERT_DISK(virtual_media);
        }

        public KorvetBIOS(IDiskParameters dpb)
        {
            DPB = dpb;
        }

        public KorvetBIOS(IDiskParameters dpb, ByteStorage virtual_media)
        {
            INSERT_DISK(virtual_media);
            DPB = dpb;
        }



        private void initialize_media()
        {
            if (Media.Count < 128)
            {
                throw new Exception("Invalid media size.");
            }

            // LOAD PHYSICAL DISK PARAMETERS
            SecSize = Media.GetByte(10);
            InSide = Media.GetByte(11);
            SecPerTrack = Media.GetByte(12);
            TrkPerDisk = Media.GetByte(15);





            MediaChanged = false;

        }


        public ByteStorage BUFFER { get; set; } = new ByteStorage();



        public void INSERT_DISK(ByteStorage virtual_media)
        {
            Media = virtual_media;
            MediaChanged = true;
        }


        public IDiskParameters SELDSK()
        {
            if (Media.Count == 0)
            {
                throw new Exception("no disk inserted");
            }

            initialize_media();
            return DPB;


        }

        public void SETTRK(int track)
        {
            Track = track;
        }

        public void SETSEC(int sector)
        {
            Sector = SECTRAN(sector);
        }

        public int SECTRAN(int sector)
        {
            // TODO: do the skew
            return sector;
        }

        public int READ()
        {
            // Sets the BUFFER 
            // Returns A=0 for OK, 1 for unrecoverable error, 0FFh if media changed

            // return [RecordSize] bytes of the requested Record (128-bytes logical sector)

            if (MediaChanged)
            {
                return 0xFF;
            }

            // calculate the position
            int offset = (DPB.SPT * Track + Sector) * RecordSize;

            if (offset >= Media.Count)
            {
                return 1;
            }

            BUFFER = Media.GetRange(offset, RecordSize);

            return 0;
        }

        public int WRITE(int C)
        {
            /*
             * Modes:
             * 0 - write can be deferred (anything)
             * 1 - write immediately (directory update)
             * 2 - write can be deferred, and no pre-reading is necessary (i.e. it's the first record of a physical sector)
             * 
             */
            // Writes the BUFFER content to the set TRK and SEC
            // Returns A=0 for OK, 1 for unrecoverable error, 2 if disc is readonly, 0FFh if media changed.

            if (BUFFER.Count != RecordSize)
            {
                throw new Exception("Record size mismatch");
            }

            if (MediaChanged)
            {
                return 0xFF; // error FF: media has been changed in the drive
            }

            throw new Exception("Code 2. Read-only disk.");


        }


    }
}
