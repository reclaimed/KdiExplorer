namespace KorvetDiskImage.Interfaces
{
    internal interface IBlockDevice
    {

        public void INSERT_DISK(ByteStorage virtual_media);

        public IDiskParameters SELDSK();

        public void SETTRK(int track);

        public void SETSEC(int sector);

        public int SECTRAN(int sector);

        public int READ();

        public int WRITE(int C);

        public ByteStorage BUFFER { get; set; }
    }
}
