
using KorvetDiskImage.Interfaces;
using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage.Cpm22
{

    /*
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * Soviet CP/M micros has Disk Parameter Block written on a disk.
     * 
     * 
     * 
     * I believe it was done to make future upgrades easier, but in fact made floppies even less reliable.
     * 
     * 
     * 
     * 
     */
    public class KorvetDPB : IDiskParameters
    {
        private ByteStorage Media;

        public KorvetDPB(ByteStorage media)
        {
            Media = media;


            // TEST CHECKSUM
            byte sum = 0x66;
            for (int i = 0; i < 31; i++)
            {
                sum = (byte)(0xFF & (sum + Media.GetByte(i)));
            }

            if (Media.GetByte(31) != sum)
            {
                throw new Exception("Korvet DPH: checksum error");
            }

        }

        //// ИНФОРМАЦИЯ ДЛЯ ЗАГРУЗЧИКА (если нули - диск не системный)
        //public int LoadAddress { get { return BootRecord.GetWord(00); } }    //  0:  DW LoadAdress ; адрес в памяти, с которого начинается загрузка ОС
        //public int RunAddress { get { return BootRecord.GetWord(02); } }     //  2:  DW RunAdress  ; адрес в памяти, куда передается управление после; загрузки
        //public int Count { get { return BootRecord.GetWord(04); } }          //  4:  DW Count      ; число загружаемых физических секторов
        //public new byte CRC { get { return BootRecord.GetByte(31); } }   //31: DB CheckSum; контрольная сумма служебной информации (CS[1 - 31]+66H)


        //// ФИЗИЧЕСКИЕ ПАРАМЕТРЫ ДИСКА
        //public byte SizeDisk { get { return BootRecord.GetByte(06); } }      //  6: 00 :DB SizeDisk   ; если значение байта 1 - 8" диск, если 0; - 5.25"
        //public byte Density { get { return BootRecord.GetByte(07); } }       //  7: 01 :DB Density    ; способ записи: 0 - FM; 1 - MFM
        //public byte TpI { get { return BootRecord.GetByte(08); } }           //  8: 01 :DB TpI: tracks per inch 0 - 48 TpI; 1 - 96 TpI, 2 - 135 TpI
        //public byte SkewFactor { get { return BootRecord.GetByte(09); } }    //  9: 01 :DB SkewFactor: x==1 нету; иначе - значения в 32..(32+x) содержат таблицу перевода
        //public List<byte> SkewMap { get { return BootRecord.GetRange(32, SkewFactor).AsList; } } // 32..127: карта смещения секторов (размер карты содержится в SkewFactor)



        //// DISK PARAMETER HEADER
        //public int SecSize { get { return BootRecord.GetByte(10); } }        // 10: 03 :DB SecSize; размер физического сектора:0 - 128 байт, 1 - 256 байт, 2 - 512 байт и  3 - 1024
        //public byte InSide { get { return BootRecord.GetByte(11); } }        // 11: 01 :DB InSide ; информация о сторонах диска: 0 - односторонний диск, 1 - двухсторонний
        //public int SecPerTrack { get { return BootRecord.GetWord(12); } }    // 12: 05 :DW SecPerTrack; число физических секторов на дорожке
        //public int TrkPerDisk { get { return BootRecord.GetWord(14); } }     // 14: 80 :DW TrkPerDisk; число дорожек на диске(с одной стороны)

        // DISK PARAMETER BLOCK
        public ushort SPT { get { return Media.GetWord(16); } }    //16: DW SPT; Number of 128-byte records per track (default 40)
        public byte BSH { get { return Media.GetByte(18); } }   //18: DB BSH; Block shift. 3 => 1k, 4 => 2k, 5 => 4k.... (default 4)        
        public byte BLM { get { return Media.GetByte(19); } }   //19: DB BLM; Block mask. 7 => 1k, 0Fh => 2k, 1Fh => 4k... (defaul: 15; logical_sectors_number-1)
        public byte EXM { get { return Media.GetByte(20); } }   //20: DB EXM; маска размера (default 0; вспомогательная величина для определения номера extent) EXM = (BLM+1) * 128 / 1024 - 1 - [DSM/256]        
        public ushort DSM { get { return Media.GetWord(21); } }    //21: DW DSM; (no. of blocks on the disc)-1 (default 394)        
        public ushort DRM { get { return Media.GetWord(23); } }    //23: DW DRM; (no. of directory entries)-1 (default: 127)        
        public byte AL0 { get { return Media.GetByte(25); } }   //25: DB AL0; определяет, какие кластеры  зарезервированы под директорию (default: 192)        
        public byte AL1 { get { return Media.GetByte(26); } }   //26: DB AL1; -- // --        
        public ushort CKS { get { return Media.GetWord(27); } }    //27: DW CKS; Checksum vector size, 0 for a fixed disc (default: 32; 0 for a fixed disk; round((DRM+1)/4)    )
        public ushort OFF { get { return Media.GetWord(29); } }    //29: DW OFS; Offset, number of reserved (for operating system) tracks








    }
}
