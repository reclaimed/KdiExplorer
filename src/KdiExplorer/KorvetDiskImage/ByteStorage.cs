using KorvetDiskImage.Exceptions;

namespace KorvetDiskImage
{
    public class ByteStorage
    {
        private List<byte> Data;
        private void ValidateAddressRange(int addr, int length)
        {
            if (addr < 0)
            {
                throw new ByteStorageException($"Negative address: {addr}");
            }

            if (addr > Data.Count)
            {
                throw new ByteStorageException($"Out of range: expected address within {Data.Count - 1}; got address {addr}");
            }

            if ((addr + length) > Data.Count)
            {
                throw new ByteStorageException($"Out of range: range ({addr},{length}) overlaps storage size of {Data.Count - 1}");
            }
        }


        // TODO: image file name for save/load operations
        public bool IsBigEndian { get; private set; } = false;
        public bool IsLittleEndian { get { return !IsBigEndian; } }
        public List<byte> AsList { get { return Data; } }
        public byte[] AsArray { get { return Data.ToArray(); } }
        public int Count { get { return Data.Count; } }

        public string SaveDirectory { get; private set; } = Directory.GetCurrentDirectory();
        public string Filename { get; private set; } = $"image_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.kdi";


        // zero-sized storage
        public ByteStorage(bool bigEndian = false)
        {
            IsBigEndian = bigEndian;
            Data = new List<byte>();
        }

        // given-sized storage
        public ByteStorage(int count, byte filler, bool bigEndian = false)
        {
            IsBigEndian = bigEndian;
            Data = new List<byte>(count);

            for (int i = 0; i < count; i++) Data.Add(filler);


        }

        // initialize with size and values of an array
        public ByteStorage(byte[] bytes, bool bigEndian = false)
        {


            IsBigEndian = bigEndian;

            Data = new List<byte>(bytes);
        }

        // initialize with size and values of a list
        public ByteStorage(List<byte> bytes, bool bigEndian = false)
        {
            // create from list

            IsBigEndian = bigEndian;
            Data = bytes;
        }

        // initialize with a binary file (no size check!)
        public ByteStorage(string path, bool bigEndian = false)
        {
            IsBigEndian = bigEndian;

            Data = new List<byte>(File.ReadAllBytes(path));

            // update properties            
            var fileinfo = new FileInfo(path);
            SaveDirectory = fileinfo.DirectoryName;
            Filename = fileinfo.Name;
        }


        public ByteStorage SetByte(int addr, byte value)
        {

            ValidateAddressRange(addr, 1);

            Data[addr] = value;
            return this;
        }


        public ByteStorage SetWord(int addr, ushort value)
        {

            ValidateAddressRange(addr, 2);

            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian == IsLittleEndian)
            {
                Data[addr] = bytes[0];
                Data[addr+1] = bytes[1];
            }
            else
            {
                Data[addr] = bytes[1];
                Data[addr+1] = bytes[0];
            }

            return this;

        }



        public byte GetByte(int addr)
        {
            ValidateAddressRange(addr, 1);

            return Data[addr];
        }

        public UInt16 GetWord(int addr)
        {

            ValidateAddressRange(addr, 2);

            // if IsBigEndian is set, use the BigEndian byte->integer translation
            return IsBigEndian ? (UInt16)(Data[addr] << 8 | Data[addr + 1]) : (UInt16)(Data[addr + 1] << 8 | Data[addr]);
        }

        public ByteStorage GetRange(int addr, int length)
        {

            ValidateAddressRange(addr, length);

            // return a new RawMedia with a slice of data
            return new ByteStorage(
                Data.GetRange(addr, length),
                IsBigEndian
                );
        }


        public ByteStorage AddRange(ByteStorage bytes)
        {
            Data.AddRange(bytes.AsList);
            return this;
        }

        public ByteStorage AddRange(byte[] bytes)
        {
            Data.AddRange(bytes);
            return this;

        }


        public ByteStorage AddRange(List<byte> bytes)
        {
            Data.AddRange(bytes);
            return this;
        }


    }
}